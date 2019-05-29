using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Katbot.Attributes;
using Katbot.Extensions;
using Katbot.Results;
using Qmmands;

namespace Katbot.Entities
{
    public abstract class KatbotModuleBase : ModuleBase<KatbotCommandContext>
    {
        public Task<RestUserMessage> ReplyAsync(string content = null, EmbedBuilder embed = null,
            RequestOptions options = null)
        {
            return Context.ReplyAsync(content, embed, options);
        }

        public ActionResult Image(params FileAttachment[] attachments)
        {
            return new OkResult((string) null, attachments);
        }

        public ActionResult Image(string title, string imageUrl)
        {
            return Ok(e =>
            {
                e.ImageUrl = imageUrl;
                e.Title = title;
            });
        }

        public ActionResult Ok(string content, params FileAttachment[] attachments)
        {
            return Context.Command.HasAttribute<DontEmbedAttribute>()
                ? new OkResult(content, attachments)
                : Ok(new EmbedBuilder().WithDescription(content), attachments);
        }

        public ActionResult Ok(EmbedBuilder builder, params FileAttachment[] attachments)
        {
            if (builder.Footer == null && !Context.Command.HasAttribute<DontAttachFooterAttribute>()) builder.WithRequesterFooter(Context);
            if (builder.Timestamp == null && !Context.Command.HasAttribute<DontAttachTimestampAttribute>()) builder.WithCurrentTimestamp();
            return new OkResult(builder.WithColor(Context.Invoker.GetHighestRoleColourOrDefault()), attachments);
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

        public ActionResult NoResult()
        {
            return new NoResult();
        }
    }
}