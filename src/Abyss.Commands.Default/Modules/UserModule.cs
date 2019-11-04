using Abyssal.Common;
using Disqord;
using Humanizer;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    [Name("User Information")]
    [Description("Commands that help you interact with other Discord users.")]
    public class UserModule : AbyssModuleBase
    {
        private readonly AbyssConfig _config;

        public UserModule(AbyssConfig config)
        {
            _config = config;
        }

        [Command("avatar")]
        [Description("Grabs the avatar for a user.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter | ResponseFormatOptions.DontAttachTimestamp)]
        public Task<ActionResult> Command_GetAvatarAsync(
            [Name("User")]
            [Description("The user who you wish to get the avatar for.")]
            [DefaultValueDescription("The user who invoked this command.")]
            CachedMember? target = null)
        {
            target ??= Context.Invoker;
            return Ok(a =>
            {
                a.WithAuthor(target.ToEmbedAuthorBuilder());
                a.ImageUrl = target.GetAvatarUrl();
                a.Description =
                    $"{UrlHelper.CreateMarkdownUrl("128", target.GetAvatarUrl(size: 128))} | " +
                    $"{UrlHelper.CreateMarkdownUrl("256", target.GetAvatarUrl(size: 256))} | " +
                    $"{UrlHelper.CreateMarkdownUrl("1024", target.GetAvatarUrl(size: 1024))} | " +
                    $"{UrlHelper.CreateMarkdownUrl("2048", target.GetAvatarUrl(size: 2048))}";
            });
        }

        [Command("user", "userinfo")]
        [Description("Grabs information around a member.")]
        public Task<ActionResult> Command_GetUserInfoAsync(
            [Name("Member")]
            [Description("The user to get information for.")]
            [DefaultValueDescription("The user who invoked this command.")]
            CachedMember? member = null)
        {
            member ??= Context.Invoker;

            var embed = new LocalEmbedBuilder
            {
                ThumbnailUrl = member.GetAvatarUrl(),
                Color = member.GetHighestRoleColourOrDefault(),
                Author = member.ToEmbedAuthorBuilder()
            };

            var desc = new StringBuilder();

            static string FormatActivity(Activity activity)
            {
                if (activity is SpotifyActivity spotify)
                {
                    return $"Listening to {spotify.TrackTitle} by {spotify.Artists.Humanize()}";
                }
                return $"{activity.Type.Humanize()} {activity.Name}";
            }

            var effectiveColor = member.GetHighestRoleColour();

            desc.AppendLine($"**- ID:** {member.Id}");
            //desc.AppendLine($"**- Created:** {FormatOffset(member.)}");
            if (member.JoinedAt != null) desc.AppendLine($"**- Joined:** {FormatHelper.FormatTime(member.JoinedAt)}");
            desc.AppendLine(
                $"**- Position:** {(member.Hierarchy == int.MaxValue ? "Server Owner" : member.Hierarchy.Ordinalize())}");
            desc.AppendLine($"**- Deafened:** {(member.IsDeafened ? "Yes" : "No")}");
            desc.AppendLine($"**- Muted:** {(member.IsMuted ? "Yes" : "No")}");
            desc.AppendLine($"**- Nickname:** {member.Nick ?? "None"}");
            desc.AppendLine($"**- Voice Status:** {GetVoiceChannelStatus(member)}");
            if (member.Activity != null)
                desc.AppendLine($"**- Activity:** {FormatActivity(member.Activity)}");
            desc.AppendLine($"**- Status:** {_config.Emotes.GetEmoteFromActivity(member.Status)} {member.Status.Humanize()}");
            desc.AppendLine($"**- Mutual servers:** {Context.Invoker.MutualGuilds.Count}");
            //if (member. != null) desc.AppendLine($"**- Nitro membership since: {FormatOffset(member.PremiumSince.Value)}");
            //if (member.ActiveClients != null) desc.AppendLine($"**- Active on:** {string.Join(", ", member.ActiveClients)}");
            if (effectiveColor != null) desc.AppendLine($"**- Colour:** {effectiveColor.Value.ToString()} (R {effectiveColor.Value.R}, G {effectiveColor.Value.G}, B {effectiveColor.Value.B})");
            embed.Description = desc.ToString();

            var roles = member.Roles.Values.Where(r => !(r.Id == member.Guild.Id));
            var socketRoles = roles as CachedRole[] ?? roles.ToArray();

            embed.AddField($"{(socketRoles.Length > 0 ? socketRoles.Length.ToString() : "No")} role{(socketRoles.Length == 1 ? "" : "s")}", socketRoles.Length > 0 ? string.Join(", ", socketRoles.Select(r => r.Name)) : AbyssHostedService.ZeroWidthSpace);

            return Ok(embed);
        }

        [Command("hug")]
        [Description("Gives them all your hugging potential.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter | ResponseFormatOptions.DontAttachTimestamp)]
        public Task<ActionResult> Command_HugUserAsync([Name("Member")] [Description("The user to hug.")]
            CachedMember hugee)
        {
            if (hugee.Id == Context.Invoker.Id) return Ok("You hug yourself.");

            return Ok(e =>
            {
                e.Description = $"**{Context.Invoker.DisplayName}** hugs **{hugee.DisplayName}**!";
            });
        }

        private static string GetVoiceChannelStatus(CachedMember user)
        {
            return user.VoiceChannel == null ? "Not in a voice channel" : $"In {user.VoiceChannel.Name}";
        }
    }
}