using Abyssal.Common;
using Qmmands;
using Disqord;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Abyss
{
    /// <summary>
    ///     The base class for all Abyss modules.
    /// </summary>
    public abstract class AbyssModuleBase : ModuleBase<AbyssRequestContext>
    {
        #pragma warning disable 8618
        public ILoggerFactory LoggerFactory { get; set; }
        #pragma warning restore
        
        public ILogger Logger => LoggerFactory.CreateLogger(this.GetType().Name);


        /// <summary>
        ///     Sends a message to the context channel.
        /// </summary>
        /// <param name="content">The string context of the message.</param>
        /// <param name="embed">The embed of the message.</param>
        /// <param name="options">Request options.</param>
        /// <returns>A Task representing the asynchronous message create call.</returns>
        public Task ReplyAsync(string? content = null, LocalEmbedBuilder? embed = null,
            RestRequestOptions? options = null)
        {
            return Context.ReplyAsync(content, embed, options);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents sending an attachment to the channel.
        /// </summary>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="ActionResult"/> that represents sending the specified attachments to the context channel.</returns>
        public static ActionResult Attachment(params LocalAttachment[] attachments)
        {
            return new OkResult((string?) null, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents sending an image with a URL to the channel.
        /// </summary>
        /// <param name="title">The (optional) title to send with the image.</param>
        /// <param name="imageUrl">The image URL.</param>
        /// <returns>An <see cref="ActionResult"/> that represents sending the specified image to the context channel.</returns>
        public ActionResult Image(string? title, string imageUrl)
        {
            return Ok(e =>
            {
                e.ImageUrl = imageUrl;
                e.Title = title;
            });
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents replying with the OK Hand emoji.
        /// </summary>
        /// <returns>A <see cref="ReplySuccessResult"/> that represents sending the "OK Hand" emoji to the context channel.</returns>
        public static ReplySuccessResult Ok() => new ReplySuccessResult();

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents reacting to the invocation message with the OK Hand emoji.
        /// </summary>
        /// <returns>A <see cref="ReactSuccessResult"/> that represents reacting to the invocation message with the OK Hand emoji.</returns>
        public static ReactSuccessResult OkReaction() => new ReactSuccessResult();

        /// <summary>
        ///     Represents an <see cref="ActionResult"/> that represents replying to the invocation message with the specified text and attachments.
        /// </summary>
        /// <param name="content">The string message to send.</param>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="OkResult"/> that represents replying to the invocation message with the specified text and attachments.</returns>
        public ActionResult Ok(string content, params LocalAttachment[] attachments)
        {
            return new OkResult(content, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents replying to a context with the specified embed and attachments.
        /// </summary>
        /// <param name="context">The context to reply to.</param>
        /// <param name="builder">The embed builder to send.</param>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="ActionResult"/> that represents replying to the specified context with the specified embed and attachments.</returns>
        public static ActionResult Ok(AbyssRequestContext context, LocalEmbedBuilder builder, params LocalAttachment[] attachments)
        {
            bool attachFooter = false;
            if (builder.Footer == null)
            {
                if (context.Command.GetType().HasCustomAttribute<ResponseFormatOptionsAttribute>(out var at))
                {
                    attachFooter = !at!.Options.HasFlag(ResponseFormatOptions.DontAttachFooter);
                }
                else attachFooter = true;
            }

            bool attachTimestamp = false;
            if (builder.Timestamp == null)
            {
                if (context.Command.GetType().HasCustomAttribute<ResponseFormatOptionsAttribute>(out var at0))
                {
                    attachTimestamp = !at0!.Options.HasFlag(ResponseFormatOptions.DontAttachTimestamp);
                }
                else attachTimestamp = true;
            }

            if (attachFooter) builder.WithRequesterFooter(context);
            if (attachTimestamp) builder.WithTimestamp(DateTimeOffset.Now);
            return new OkResult(builder.WithColor(context.BotMember.GetHighestRoleColourOrDefault()), attachments);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents replying to the invocation context with the specified embed and attachments.
        /// </summary>
        /// <param name="builder">The embed builder to send.</param>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="ActionResult"/> that represents replying to the specified context with the specified embed and attachments.</returns>
        public ActionResult Ok(LocalEmbedBuilder builder, params LocalAttachment[] attachments)
        {
            return Ok(Context, builder, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents replying to a context with the specified embed and attachments.
        /// </summary>
        /// <param name="actor">An actor which mutates the embed to send.</param>
        /// <param name="attachments">The attachments to send.</param>
        /// <returns>An <see cref="ActionResult"/> that represents replying to the specified context with the specified embed and attachments.</returns>
        public ActionResult Ok(Action<LocalEmbedBuilder> actor, params LocalAttachment[] attachments)
        {
            var eb = new LocalEmbedBuilder();
            actor(eb);
            return Ok(eb, attachments);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that represents invalidity of the request parameters.
        /// </summary>
        /// <param name="reason">The reason as to why the request parameters are invalid.</param>
        /// <returns>A <see cref="BadRequestResult"/> that represents replying to the specified context with the specified error message.</returns>
        public static ActionResult BadRequest(string reason)
        {
            return new BadRequestResult(reason);
        }

        /// <summary>
        ///     Returns an <see cref="ActionResult"/> that does nothing.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> that does nothing.</returns>
        public static ActionResult Empty()
        {
            return new EmptyResult();
        }
    }
}