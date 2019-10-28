using Disqord;
using Disqord.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    public static class DiscordExtensions
    {
        public static async Task<bool> TrySendMessageAsync(this IMessageChannel messageChannel, string? message = null, bool isTts = false, Embed? embed = null, RestRequestOptions? options = null)
        {
            try
            {
                await messageChannel.SendMessageAsync(message, isTts, embed, options);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> TryDeleteAsync(this IDeletable deletable, RestRequestOptions? options = null)
        {
            try
            {
                await deletable.DeleteAsync(options).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static ITextChannel? GetDefaultChannel(this CachedGuild guild)
        {
            if (guild.TextChannels.Count == 0) return null;
            var defaultChannel = guild.GetDefaultChannel();
            if (defaultChannel != null) return defaultChannel;

            // find channel
            var firstOrDefault = guild.TextChannels.Values.FirstOrDefault(c =>
                c.Name.Equals("general", StringComparison.OrdinalIgnoreCase)
                && guild.CurrentMember.GetPermissionsFor(c).SendMessages);

            return firstOrDefault ?? guild.TextChannels.FirstOrDefault(c => guild.CurrentMember.GetPermissionsFor(c.Value).SendMessages).Value;
        }

        public static LogLevel ToMicrosoftLogLevel(this LogMessageSeverity logSeverity)
        {
            return logSeverity switch
            {
                LogMessageSeverity.Information => LogLevel.Information,
                LogMessageSeverity.Error => LogLevel.Error,
                LogMessageSeverity.Debug => LogLevel.Debug,
                LogMessageSeverity.Critical => LogLevel.Critical,
                LogMessageSeverity.Trace => LogLevel.Trace,
                LogMessageSeverity.Warning => LogLevel.Warning,
                _ => LogLevel.Information
            };
        }
    }
}