using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Core.Extensions
{
    public static class DiscordExtensions
    {
        public static async Task<bool> TrySendMessageAsync(this IMessageChannel messageChannel, string message = null, bool isTts = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await messageChannel.SendMessageAsync(message, isTts, embed, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryDeleteAsync(this IDeletable deletable, RequestOptions options = null)
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

        public static ITextChannel GetDefaultChannel(this SocketGuild guild)
        {
            if (guild.DefaultChannel != null) return guild.DefaultChannel;
            if (guild.TextChannels.Count == 0) return null;

            // find channel
            var firstOrDefault = guild.TextChannels.FirstOrDefault(c =>
                c.Name.Equals("general", StringComparison.OrdinalIgnoreCase)
                && guild.CurrentUser.GetPermissions(c).SendMessages);

            return firstOrDefault ?? guild.TextChannels.FirstOrDefault(c => guild.CurrentUser.GetPermissions(c).SendMessages);
        }

        public static LogLevel ToMicrosoftLogLevel(this LogSeverity logSeverity)
        {
            return logSeverity switch
            {
                LogSeverity.Critical => LogLevel.Critical,

                LogSeverity.Error => LogLevel.Error,

                LogSeverity.Warning => LogLevel.Warning,

                LogSeverity.Info => LogLevel.Information,

                LogSeverity.Verbose => LogLevel.Trace,

                LogSeverity.Debug => LogLevel.Debug,

                _ => LogLevel.Information,
            };
        }
    }
}