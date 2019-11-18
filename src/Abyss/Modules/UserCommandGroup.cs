using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Humanizer;
using Qmmands;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Abyss
{
    [Name("User information")]
    [Group("user")]
    public class UserCommandGroup : AbyssModuleBase
    {
        private readonly AbyssConfig _config;

        public UserCommandGroup(AbyssConfig config)
        {
            _config = config;
        }

        [Command("avatar")]
        [Description("View the avatar of a user.")]
        public Task<AbyssResult> Command_GetAvatarAsync(
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

        [Command("", "info")]
        [Description("View information about a member.")]
        public Task<AbyssResult> Command_GetUserInfoAsync(
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
            if (member.Presence?.Activity != null)
                desc.AppendLine($"**- Activity:** {FormatActivity(member.Presence.Activity)}");
            if (member.Presence != null) desc.AppendLine($"**- Status:** {_config.Emotes.GetStatusEmote(member.Presence.Status)} {member.Presence.Status.Humanize()}");
            
            if (user.MutualGuilds != null) desc.AppendLine($"**- Mutual servers:** {user.MutualGuilds.Count}");
            if (effectiveColor != null) desc.AppendLine($"**- Colour:** {effectiveColor.Value.ToString()} (R {effectiveColor.Value.R}, G {effectiveColor.Value.G}, B {effectiveColor.Value.B})");
            embed.Description = desc.ToString();

            var roles = member.Roles.Values.Where(r => !(r.Id == member.Guild.Id));
            var socketRoles = roles as CachedRole[] ?? roles.ToArray();

            embed.AddField($"{(socketRoles.Length > 0 ? socketRoles.Length.ToString() : "No")} role{(socketRoles.Length == 1 ? "" : "s")}", socketRoles.Length > 0 ? string.Join(", ", socketRoles.Select(r => r.Name)) : AbyssBot.ZeroWidthSpace);

            return Ok(embed);
        }

        private static string GetVoiceChannelStatus(CachedMember user)
        {
            return user.VoiceChannel == null ? "Not in a voice channel" : $"In {user.VoiceChannel.Name}";
        }

        [Name("Nickname")]
        [Group("nick")]
        public class NicknameSubgroup : AbyssModuleBase
        {
            [Command("reset")]
            [Description("Reset (remove) the nickname for a member.")]
            [RequireBotGuildPermissions(Permission.ManageNicknames)]
            [RequireMemberGuildPermissions(Permission.ManageNicknames)]
            public Task<AbyssResult> Command_ResetNicknameAsync(
                [Name("member")] [Description("The user you would like me to reset.")] CachedMember member)
            {
                return Command_SetNicknameAsync(member, null);
            }

            [Command("set")]
            [Description("Set the nickname for a user.")]
            [RequireBotGuildPermissions(Permission.ManageNicknames)]
            [RequireMemberGuildPermissions(Permission.ManageNicknames)]
            public async Task<AbyssResult> Command_SetNicknameAsync(
                [Name("member")] [Description("The user you would like me to change nickname of.")] CachedMember target,
                [Description("The nickname to set.")] [Name("new nickname")] [Remainder] string? nickname)
            {
                try
                {
                    await target.ModifyAsync(a => a.Nick = nickname, RestRequestOptions.FromReason($"Action performed by {Context.Invoker}")).ConfigureAwait(false);
                    return SuccessReaction();
                }
                catch (DiscordHttpException e) when (e.HttpStatusCode == HttpStatusCode.Forbidden)
                {
                    return BadRequest("Not allowed to.");
                }
                catch (DiscordHttpException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest("Bad format.");
                }
            }
        }
    }
}