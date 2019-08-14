using System;
using System.Collections.Generic;
using System.Text;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Qmmands;

namespace Abyss.Shared.Hosts
{
    public static class AbyssCooldownBucketKeyGenerators
    {
        public static CooldownBucketKeyGeneratorDelegate Default = (t, ctx, services) =>
        {
            if (!(t is CooldownType ct))
            {
                throw new InvalidOperationException(
                   $"Cooldown bucket type is incorrect. Expected {typeof(CooldownType)}, received {t.GetType().Name}.");
            }

            var discordContext = ctx.ToRequestContext();

            if (discordContext.InvokerIsOwner)
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
