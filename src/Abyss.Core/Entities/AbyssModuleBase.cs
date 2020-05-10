using Abyssal.Common;
using Qmmands;
using Disqord;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    /// <summary>
    ///     The base class for all Abyss modules.
    /// </summary>
    public abstract class AbyssModuleBase : ModuleBase<AbyssRequestContext>
    {
        /// <summary>
        ///     Sends a message to the context channel.
        /// </summary>
        /// <param name="content">The string context of the message.</param>
        /// <param name="embed">The embed of the message.</param>
        /// <param name="options">Request options.</param>
        /// <returns>A Task representing the asynchronous message create call.</returns>
        public Task ReplyAsync(string? content = null, EmbedBuilder? embed = null,
            RestRequestOptions? options = null)
        {
            return Context.ReplyAsync(content, embed, options);
        }
    }
}