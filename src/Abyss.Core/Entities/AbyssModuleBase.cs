using Abyss.Core.Extensions;
using Abyss.Core.Results;
using Abyssal.Common;
using Discord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Entities
{
    public abstract class AbyssModuleBase : ModuleBase<AbyssRequestContext>
    {
        public Task ReplyAsync(string? content = null, EmbedBuilder? embed = null,
            RequestOptions? options = null)
        {
            return Context.ReplyAsync(content, embed, options);
        }

        public ActionResult Image(params FileAttachment[] attachments)
        {
            return new OkResult((string?) null, attachments);
        }

        public ActionResult Image(string? title, string imageUrl)
        {
            return Ok(e =>
            {
                e.ImageUrl = imageUrl;
                e.Title = title;
            });
        }

        public OkResult Text(string raw) => new OkResult(raw);

        public ReplySuccessResult Ok() => new ReplySuccessResult();

        public ReactSuccessResult OkReaction() => new ReactSuccessResult();

        public ActionResult Ok(string content, params FileAttachment[] attachments)
        {
            return (Context.Command.GetType().HasCustomAttribute<ResponseFormatOptionsAttribute>(out var at) && at!.Options.HasFlag(ResponseFormatOptions.DontEmbed))
                ? new OkResult(content, attachments)
                : Ok(new EmbedBuilder().WithDescription(content), attachments);
        }

        public ActionResult Ok(EmbedBuilder builder, params FileAttachment[] attachments)
        {
            bool attachFooter = false;
            if (builder.Footer == null)
            {
                if (Context.Command.GetType().HasCustomAttribute<ResponseFormatOptionsAttribute>(out var at))
                {
                    attachFooter = !at!.Options.HasFlag(ResponseFormatOptions.DontAttachFooter);
                }
                else attachFooter = true;
            }

            bool attachTimestamp = false;
            if (builder.Timestamp == null)
            {
                if (Context.Command.GetType().HasCustomAttribute<ResponseFormatOptionsAttribute>(out var at0))
                {
                    attachTimestamp = !at0!.Options.HasFlag(ResponseFormatOptions.DontAttachTimestamp);
                }
                else attachTimestamp = true;
            }

            if (attachFooter) builder.WithRequesterFooter(Context);
            if (attachTimestamp) builder.WithCurrentTimestamp();
            return new OkResult(builder.WithColor(Context.BotUser.GetHighestRoleColourOrDefault()), attachments);
        }

        public ActionResult Ok(Action<EmbedBuilder> actor, params FileAttachment[] attachments)
        {
            var eb = new EmbedBuilder();
            actor(eb);
            return Ok(eb, attachments);
        }

        public ActionResult BadRequest(string reason)
        {
            return new BadRequestResult(reason);
        }

        public ActionResult Empty()
        {
            return new EmptyResult();
        }
    }
}