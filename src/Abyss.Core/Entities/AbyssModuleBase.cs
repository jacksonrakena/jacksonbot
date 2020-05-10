using Abyssal.Common;
using Qmmands;
using Disqord;
using System;
using System.Threading.Tasks;
using Disqord.Rest;

namespace Abyss
{
    /// <summary>
    ///     The base class for all Abyss modules.
    /// </summary>
    public abstract class AbyssModuleBase : ModuleBase<AbyssCommandContext>
    {
        /// <summary>
        ///     Sends a message to the context channel.
        /// </summary>
        /// <param name="content">The string context of the message.</param>
        /// <param name="embed">The embed of the message.</param>
        /// <param name="options">Request options.</param>
        /// <param name="mentions">Mentions options.</param>
        /// <returns>A Task representing the asynchronous message create call.</returns>
        public Task ReplyAsync(string content = null, LocalMentions mentions = null, LocalEmbedBuilder embed = null,
            RestRequestOptions options = null)
        {
            
            return Context.ReplyAsync(content, mentions, embed, options);
        }

        public Color GetColor() => Context.GetColor();
    }
}