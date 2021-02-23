using System;
using Qmmands;
using Qmmands.Delegates;

namespace Lament.Discord
{
    public static class CooldownBucketKeyGenerators
    {
        public static CooldownBucketKeyGeneratorDelegate Default = (t, ctx) =>
        {
            if (!(t is CooldownType ct))
            {
                throw new InvalidOperationException(
                    $"Cooldown bucket type is incorrect. Expected {typeof(CooldownType)}, received {t.GetType().Name}.");
            }

            var discordContext = (LamentCommandContext) ctx;

            if (discordContext.User.Id == discordContext.Bot.CurrentApplication.GetAsync().GetAwaiter().GetResult().Owner.Id)
                return null; // Owners have no cooldown

            return ct switch
            {
                CooldownType.Server => discordContext.Guild.Id.ToString(),

                CooldownType.Channel => discordContext.Channel.Id.ToString(),

                CooldownType.User => discordContext.User.Id.ToString(),

                CooldownType.Global => "Global",

                _ => throw new ArgumentOutOfRangeException()
            };
        };
    }
}