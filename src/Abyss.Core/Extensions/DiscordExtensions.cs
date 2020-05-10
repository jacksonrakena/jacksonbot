using Disqord;
using Disqord.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Rest;

namespace Abyss
{
    /// <summary>
    ///     Extensions for the Discord library.
    /// </summary>
    public static class DiscordExtensions
    {
        /// <summary>
        ///     Attempts to delete the current deletable object.
        /// </summary>
        /// <param name="deletable">The current deletable object.</param>
        /// <param name="options">Options for this REST request.</param>
        /// <returns>A Task representing whether the asynchronous delete call succeeded.</returns>
        public static async Task<bool> TryDeleteAsync(this IDeletable deletable, RestRequestOptions options = null)
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