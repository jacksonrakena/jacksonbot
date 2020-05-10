using System;
using Qmmands;
using Qmmands.Delegates;

namespace Abyss
{
    public static class AbyssCooldownBucketKeyGenerators
    {
        public static CooldownBucketKeyGeneratorDelegate Default = (t, ctx) =>
        {
            if (!(t is CooldownType ct))
            {
                throw new InvalidOperationException(
                   $"Cooldown bucket type is incorrect. Expected {typeof(CooldownType)}, received {t.GetType().Name}.");
            }

            var discordContext = ctx.ToCommandContext();

            if (discordContext.Invoker.Id == discordContext.Bot.CurrentApplication.GetAsync().GetAwaiter().GetResult().Owner.Id)
                return null; // Owners have no cooldown

            return ct switch
            {
                CooldownType.Server => discordContext.Guild.Id.ToString(),

                CooldownType.Channel => discordContext.Channel.Id.ToString(),

                CooldownType.User => discordContext.Invoker.Id.ToString(),

                CooldownType.Global => "Global",

                _ => throw new ArgumentOutOfRangeException()
            };
        };
    }
}
