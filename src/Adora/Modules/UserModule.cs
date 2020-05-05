using Abyssal.Common;
using Disqord;
using Humanizer;
using Qmmands;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adora
{
    [Name("User information")]
    public class UserModule : AdoraModuleBase
    {
        [Command("avatar")]
        [Description("View the avatar of a user.")]
        public Task<AdoraResult> Command_GetAvatarAsync(
            [Name("user")]
            [Description("The user who you wish to get the avatar for.")]
            [DefaultValueDescription("The user who invoked this command.")]
            [Remainder]
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

        [Command("userinfo", "user")]
        [Description("View information about a member.")]
        public Task<AdoraResult> Command_GetUserInfoAsync(
            [Name("member")]
            [Description("The user to get information for.")]
            [DefaultValueDescription("The user who invoked this command.")]
            [Remainder]
            CachedMember? member = null)
        {
            member ??= Context.Invoker;
            var user = Context.Invoker.Client.GetUser(member.Id);

            var embed = new LocalEmbedBuilder
            {
                ThumbnailUrl = member.GetAvatarUrl(),
                Color = member.GetHighestRoleColourOrSystem(),
                Author = member.ToEmbedAuthorBuilder()
            };

            var desc = new StringBuilder();

            static string FormatActivity(Presence presence)
            {
                var strings = new List<string>();
                foreach (var act in presence.Activities)
                {
                    if (act is SpotifyActivity sact)
                    {
                        strings.Add($"{sact.TrackTitle} by {sact.Artists[0]}");
                    } else strings.Add($"{act.Type} {act.Name}");
                }
                return string.Join(", ", strings);
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
            if (member.Presence != null && member.Presence.Activities.Count > 0)
                desc.AppendLine($"**- Activity:** {FormatActivity(member.Presence)}");
            if (member.Presence != null) desc.AppendLine($"**- Status:** {member.Presence.Status.Humanize()}");
            
            if (user.MutualGuilds != null) desc.AppendLine($"**- Mutual servers:** {user.MutualGuilds.Count}");
            if (effectiveColor != null) desc.AppendLine($"**- Colour:** {effectiveColor.Value.ToString()} (R {effectiveColor.Value.R}, G {effectiveColor.Value.G}, B {effectiveColor.Value.B})");
            embed.Description = desc.ToString();

            var roles = member.Roles.Values.Where(r => !(r.Id == member.Guild.Id));
            var socketRoles = roles as CachedRole[] ?? roles.ToArray();

            embed.AddField($"{(socketRoles.Length > 0 ? socketRoles.Length.ToString() : "No")} role{(socketRoles.Length == 1 ? "" : "s")}", socketRoles.Length > 0 ? string.Join(", ", socketRoles.Select(r => r.Name)) : AdoraBot.ZeroWidthSpace);

            return Ok(embed);
        }

        private static string GetVoiceChannelStatus(CachedMember user)
        {
            return user.VoiceChannel == null ? "Not in a voice channel" : $"In {user.VoiceChannel.Name}";
        }
    }
}