using Disqord;
using Disqord.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    /// <summary>
    ///     Extensions for the Discord library.
    /// </summary>
    public static class DiscordExtensions
    {
        /// <summary>
        ///     Attempts to send a message to the specified message channel.
        /// </summary>
        /// <param name="messageChannel">The channel to send to.</param>
        /// <param name="message">The string content to send.</param>
        /// <param name="isTts">Whether this message is Text-To-Speech enabled.</param>
        /// <param name="embed">The embed to send.</param>
        /// <param name="options">Options for this REST request.</param>
        /// <returns>A Task representing whether the asynchronous message create call succeeded.</returns>
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

        /// <summary>
        ///     Attempts to delete the current deletable object.
        /// </summary>
        /// <param name="deletable">The current deletable object.</param>
        /// <param name="options">Options for this REST request.</param>
        /// <returns>A Task representing whether the asynchronous delete call succeeded.</returns>
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

        /// <summary>
        ///     Attempts to find the default channel of this guild.
        /// </summary>
        /// <param name="guild">The guild to check.</param>
        /// <returns>A default channel, if found. Otherwise the first channel where the bot has send messages permissions, or null if none exist.</returns>
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

        internal static LogLevel ToMicrosoftLogLevel(this LogMessageSeverity logSeverity)
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