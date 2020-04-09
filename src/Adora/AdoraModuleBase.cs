using Qmmands;
using Disqord;
using System;
using System.Threading.Tasks;
using Serilog;
using Disqord.Rest;

namespace Adora
{
    /// <summary>
    ///     The base class for all Adora modules.
    /// </summary>
    public abstract class AdoraModuleBase : ModuleBase<AdoraCommandContext>
    {
        protected ILogger Logger => Context.Logger;

        public Task ReplyAsync(string? content = null, LocalEmbedBuilder? embed = null,
            RestRequestOptions? options = null)
        {
            return Context.ReplyAsync(content, embed, options);
        }

        public static AdoraResult Attachment(params LocalAttachment[] attachments)
        {
            return new ContentResult((string?) null, attachments: attachments);
        }

        public static AdoraResult Image(string? title, string imageUrl)
        {
            return Ok(e =>
            {
                e.ImageUrl = imageUrl;
                e.Title = title;
            });
        }

        public static ReplySuccessResult SuccessReply() => new ReplySuccessResult();

        public static ReactSuccessResult SuccessReaction() => new ReactSuccessResult();

        public static AdoraResult Ok(string content, Action<ContentResultOptions>? resultOptions = null, params LocalAttachment[] attachments)
        {
            return new ContentResult(content, resultOptions, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="AdoraResult"/> that represents replying to a context with the specified embed and attachments.
        /// </summary>
        /// <param name="builder">The embed builder to send.</param>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="AdoraResult"/> that represents replying to the specified context with the specified embed and attachments.</returns>
        public static AdoraResult Ok(LocalEmbedBuilder builder, Action<ContentResultOptions>? resultOptions = null, params LocalAttachment[] attachments)
        {
            return new ContentResult(builder, resultOptions, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="AdoraResult"/> that represents replying to a context with the specified embed and attachments.
        /// </summary>
        /// <param name="actor">An actor which mutates the embed to send.</param>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="AdoraResult"/> that represents replying to the specified context with the specified embed and attachments.</returns>
        public static AdoraResult Ok(Action<LocalEmbedBuilder> actor, Action<ContentResultOptions>? resultOptions = null, params LocalAttachment[] attachments)
        {
            var eb = new LocalEmbedBuilder();
            actor(eb);
            return Ok(eb, resultOptions, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="AdoraResult"/> that represents invalidity of the request parameters.
        /// </summary>
        /// <param name="reason">The reason as to why the request parameters are invalid.</param>
        /// <returns>A <see cref="BadRequestResult"/> that represents replying to the specified context with the specified error message.</returns>
        public static AdoraResult BadRequest(string reason)
        {
            return new BadRequestResult(reason);
        }

        /// <summary>
        ///     Returns an <see cref="AdoraResult"/> that does nothing.
        /// </summary>
        /// <returns>An <see cref="AdoraResult"/> that does nothing.</returns>
        public static AdoraResult Empty()
        {
            return new EmptyResult();
        }
    }
}