using Abyss.Attributes;
using Abyss.Checks.Command;
using Abyss.Entities;
using Abyss.Extensions;
using Abyss.Results;
using Abyss.Services;
using Discord;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Modules
{
    [Name("Utility")]
    [Description("Commands that provide useful utilities.")]
    public class UtilityModule : AbyssModuleBase
    {
        private readonly SpoilerService _spoilerService;

        public UtilityModule(SpoilerService spoilerService)
        {
            _spoilerService = spoilerService;
        }

        [Command("Spoiler", "CreateSpoiler")]
        [Description("Creates a spoiler message, direct messaging users who would like to see the spoiler.")]
        [Example("spoiler \"The Best Bot\" I AM THE BEST BOT!", "spoiler hello_world hello world!")]
        public async Task<ActionResult> Command_CreateSpoilerAsync(
            [Name("Safe Text")] [Description("A name for the spoiler, that everyone will be able to see.")]
            string safe, [Name("Spoiler")] [Description("The content of the spoiler.")] [Remainder]
            string spoiler)
        {
            await _spoilerService.CreateSpoilerMessageAsync(Context, safe, spoiler).ConfigureAwait(false);
            return NoResult();
        }

        [Command("Echo")]
        [Description("Echoes the input text.")]
        [Example("echo THIS IS THE BEST BOT!")]
        [DontAttachFooter]
        [DontAttachTimestamp]
        [DontEmbed]
        public Task<ActionResult> Command_EchoAsync([Name("Text")] [Remainder] string echocontent)
        {
            return Ok(Context.InvokerIsOwner
                ? echocontent
                : $"{Context.Invoker}: {echocontent}");
        }

        [Command("Echod")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Description("Attempts to delete the source message, and then echoes the input text.")]
        [Example("echod THIS IS THE BEST BOT!")]
        [DontEmbed]
        public async Task<ActionResult> Command_EchoDeleteAsync([Name("Text")] [Remainder] string echocontent)
        {
            try
            {
                await Context.Message.DeleteAsync().ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }

            return Ok(Context.InvokerIsOwner
                ? echocontent
                : $"{Context.Invoker}: {echocontent}");
        }

        [Command("Delete")]
        [Description("Deletes a message by ID.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Example("delete 525827581371613184 yes", "delete 525827581371613184 no", "delete 525827581371613184")]
        [DontAttachFooter]
        [DontAttachTimestamp]
        public async Task<ActionResult> Command_DeleteMessageAsync(
            [Name("Message")] [Description("The ID of the message to delete.")]
            ulong messageId,
            [Name("Silence")] [Description("Whether to respond with confirmation of the deletion.")]
            bool silent = false)
        {
            try
            {
                var message = await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false);
                try
                {
                    var opt = RequestOptions.Default;
                    opt.AuditLogReason = $"Requested by {Context.Invoker} at {DateTime.UtcNow.ToUniversalTime():F}";
                    await message.DeleteAsync(opt).ConfigureAwait(false);
                    if (!silent) return Ok($"Deleted message {messageId}.");
                    return NoResult();
                }
                catch (Exception)
                {
                    return BadRequest("Failed to delete message. Do I have permissions?");
                }
            }
            catch (Exception)
            {
                return BadRequest("Failed to get message, did you pass an invalid ID?");
            }
        }

        [Command("Quote")]
        [Description("Quotes a message sent by a user.")]
        [Example("quote 525827581371613184")]
        [DontAttachFooter]
        public async Task<ActionResult> Command_QuoteMessageAsync([Name("ID")] [Description("The ID of the message.")]
            ulong messageId)
        {
            var message = Context.Channel.GetCachedMessage(messageId) ??
                          await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false);

            if (message == null) return BadRequest("Cannot find message.");

            var rawjumpurl = message.GetJumpUrl();
            var jumpurl = $"[Click to jump!]({rawjumpurl})";

            var embed = new EmbedBuilder();
            embed.WithAuthor(new EmbedAuthorBuilder
            {
                Name = message.Author.ToString(),
                IconUrl = message.Author.GetEffectiveAvatarUrl(),
                Url = rawjumpurl
            });
            embed.WithTimestamp(message.Timestamp);
            embed.WithColor(message.Author.GetHighestRoleColourOrDefault());
            embed.WithDescription((string.IsNullOrWhiteSpace(message.Content) ? "<< No content >>" : message.Content) +
                                  "\n\n" + jumpurl);

            var attach0 = message.Attachments.FirstOrDefault();
            if (attach0 != null) embed.WithImageUrl(attach0.Url);

            return Ok(embed);
        }
    }
}