using Humanizer;
using Qmmands;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord;
using Disqord.Rest;

namespace Abyss.Commands.Default
{
    [Name("Server Information")]
    [Description("Commands that help you interact with your server in useful and efficient ways.")]
    public class GuildModule : AbyssModuleBase
    {
        [Command("nickname", "nick")]
        [Description("Sets the current nickname for a user.")]
        [Remarks("You can provide `clear` to remove their current nickname (if any).")]
        [RequireBotGuildPermissions(Permission.ChangeNickname)]
        [RequireMemberChannelPermissions(Permission.ChangeNickname)]
        public async Task<ActionResult> Command_SetNicknameAsync(
            [Name("Target")] [Description("The user you would like me to change username of.")] CachedMember target,
            [Description("The nickname to set to. `clear` to remove one (if set).")] [Name("New Nickname")] [Remainder]
            string nickname)
        {
            if (nickname.Equals("clear", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(target.Nick))
                return BadRequest($"{target.Format()} doesn't have a nickname.");

            try
            {
                await target.ModifyAsync(a => a.Nick = nickname == "clear" ? null : nickname, RestRequestOptions.FromReason($"Action performed by {Context.Invoker}")).ConfigureAwait(false);
                return OkReaction();
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

        [Command("server", "serverinfo")]
        [Description("Grabs information around this server.")]
        public Task<ActionResult> Command_GuildInfoAsync()
        {
            var embed = new EmbedBuilder
            {
                Color = AbyssHostedService.DefaultEmbedColour,
                Author = new EmbedAuthorBuilder
                {
                    Name = "Server " + Context.Guild.Name,
                    IconUrl = Context.Guild.GetIconUrl()
                },
                ThumbnailUrl = Context.Guild.GetIconUrl()
            };
            embed.AddField("Owner", Context.Guild.Owner.ToString(), true);
            // missing in disqord - embed.AddField("Created At", UserModule.FormatOffset(Context.Guild.))
            embed.AddField("Text Channels", string.Join(", ", Context.Guild.TextChannels.Select(c => c.Value.Name)) + " (" +
                                Context.Guild.TextChannels.Count + ")", true);
            embed.AddField("Voice Channels", string.Join(", ", Context.Guild.VoiceChannels.Select(c => c.Value.Name)) + " (" +
                                Context.Guild.VoiceChannels.Count + ")", true);

            return Ok(embed);
        }

        [Command("colour", "color")]
        [Description("Grabs the colour of a role.")]
        [RunMode(RunMode.Parallel)]
        public async Task<ActionResult> Command_GetColourFromRoleAsync(
            [Name("Role")] [Description("The role you wish to view the colour of.")] [Remainder]
            CachedRole role)
        {
            if (role.Color.RawValue == 0) return BadRequest("That role does not have a colour!");

            var outStream = ImageHelper.CreateColourImage(new Rgba32(role.Color.R, role.Color.G, role.Color.B), 200, 200);
            await Context.Channel.SendMessageAsync(new LocalAttachment(outStream, "role.png"), null, embed: new EmbedBuilder()
                .WithColor(role.Color)
                .WithTitle("Role Color")
                .WithDescription(
                    $"**Hex:** {role.Color}\n\n**Red:** {role.Color.R}\n**Green:** {role.Color.G}\n**Blue:** {role.Color.B}")
                .WithImageUrl("attachment://role.png")
                .WithRequesterFooter(Context)
                .WithCurrentTimestamp()
                .Build()).ConfigureAwait(false);
            outStream.Dispose();
            return Empty();
        }

        [Command("colour", "color")]
        [Description("Grabs the colour of a user.")]
        [RunMode(RunMode.Parallel)]
        public Task<ActionResult> Command_GetColourFromUserAsync(
            [Name("User")] [Description("The user you wish to view the colour of.")] [Remainder]
            CachedMember user)
        {
            var r = user.GetHighestRoleOrDefault(a => a.Color.RawValue != 0);
            return r == null
                ? BadRequest("That user does not have a coloured role!")
                : Command_GetColourFromRoleAsync((CachedRole)r);
        }

        [Command("permissions", "perms")]
        [Description("Shows a list of a user's current guild-level permissions.")]
        public Task<ActionResult> Command_ShowPermissionsAsync(
            [Name("Target")]
            [Description("The user to get permissions for.")]
            [DefaultValueDescription("You.")]
            CachedMember? user = null)
        {
            user ??= Context.Invoker; // Get the user (or the invoker, if none specified)

            var embed = new EmbedBuilder();
            embed.WithAuthor(user);

            if (user.Id == Context.Guild.OwnerId)
            {
                embed.WithDescription("User is owner of server, and has all permissions");
                return Ok(embed);
            }

            if (user.Permissions.Administrator)
            {
                embed.WithDescription("User has Administrator permission, and has all permissions");
                return Ok(embed);
            }

            var guildPerms = user.Permissions; // Get the user's permissions

            var booleanTypeProperties = guildPerms.GetType().GetProperties()
                .Where(a => a.PropertyType.IsAssignableFrom(typeof(bool)))
                .ToList(); // Get all properties that have a property type of Boolean

            var propDict = booleanTypeProperties.Select(a => (a.Name.Humanize(), (bool)a.GetValue(guildPerms)!))
                .OrderByDescending(ab => ab.Item2 ? 1 : 0 /* Allowed permissions first */)
                .ToList(); // Store permissions as a tuple of (string Name, bool Allowed) and order by allowed permissions first

            var accept =
                propDict.Where(ab => ab.Item2).OrderBy(a => a.Item1); // Filter an array of accepted permissions
            var deny = propDict.Where(ab => !ab.Item2).OrderBy(a => a.Item2); // Filter an array of denied permissions

            var allowString = string.Join("\n", accept.Select(a => $"- {a.Item1}"));
            var denyString = string.Join("\n", deny.Select(a => $"- {a.Item1}"));
            embed.AddField("Allowed", string.IsNullOrEmpty(allowString) ? "- None" : allowString, true);
            embed.AddField("Denied", string.Join("\n", string.IsNullOrEmpty(denyString) ? "- None" : denyString), true);
            return Ok(embed);
        }

        [Command("tree", "channels")]
        [Description("Creates a tree of channels and categories in this server.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter | ResponseFormatOptions.DontAttachTimestamp)]
        public Task<ActionResult> Command_CreateChannelTreeAsync()
        {
            var guild = Context.Guild;

            var uncategorized = new StringBuilder().AppendLine("**Uncategorized**");
            var categories = new StringBuilder();
            var channelsProcessed = new List<ulong>();

            foreach (var channel in guild.TextChannels.Values.Where(c => c.CategoryId == null).Cast<CachedGuildChannel>().Concat(guild.VoiceChannels.Values.Where(a => a.CategoryId == null).Cast<CachedGuildChannel>()).OrderBy(c => c.Position))
            {
                uncategorized.AppendLine($"- {(channel is IVoiceChannel ? "" : "#")}{channel.Name} ({channel.Id})");
            }
            uncategorized.AppendLine();
            foreach (var category in guild.CategoryChannels.OrderBy(c => c.Value.Position))
            {
                var categoryBuilder = new StringBuilder().AppendLine($"**{category.Value.Name}** ({category.Value.Id})");
                foreach (var childChannel in category.Value.Channels.OrderBy(c => c.Value.Position))
                {
                    categoryBuilder.AppendLine($"- {(childChannel.Value is IVoiceChannel ? "" : "#")}{childChannel.Value.Name} ({childChannel.Value.Id})");
                }
                categories.AppendLine(categoryBuilder.ToString());
            }
            var res = uncategorized.AppendLine(categories.ToString()).ToString();
            if (res.Length >= 4000) return BadRequest("Server too big.");
            return Ok(res);
        }

        [Command("analyzeperm")]
        [Description("Analyzes a permission for a user, and sees which role grants or denies that permission to them.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter | ResponseFormatOptions.DontAttachTimestamp)]
        public Task<ActionResult> Command_AnalyzePermissionAsync(
            [Name("User")] [Description("The user to check for the specified permission.")]
            CachedMember user,
            [Name("Permission")] [Description("The permission to analyze.")] [Remainder] string permission)
        {
            user ??= Context.Invoker;

            var perm = user.Permissions.GetType().GetProperties().FirstOrDefault(a =>
                a.PropertyType.IsAssignableFrom(typeof(bool))
                && (a.Name.Equals(permission, StringComparison.OrdinalIgnoreCase)
                 || a.Name.Humanize().Equals(permission, StringComparison.OrdinalIgnoreCase)));
            if (perm == null) return BadRequest($"Unknown permission `{permission}`. :(");

            var embed = new EmbedBuilder
            {
                Author = user.ToEmbedAuthorBuilder(),
                Title = "Permission: " + perm.Name.Humanize()
            };

            if (user.Roles.Count == 0)
            {
                embed.Description = "User has no roles.";
                return Ok(embed);
            }

            if (user.Guild.OwnerId == user.Id)
            {
                embed.Description = "User is owner of this server, and has every permission regardless of roles.";
                return Ok(embed);
            }

            var p = user.Roles.Where(a => a.Value.Permissions.Administrator).ToList();
            if (p.Count > 0)
            {
                embed.Description =
                    "User has administrator, and has every permission. To deny them administrator, remove the Administrator permission from the following roles:\n" +
                    $"`{string.Join(", ", p.Select(b => b.Value.Name))}`";
                return Ok(embed);
            }

            var grantedRoles = user.Roles.Where(r => (bool)perm.GetValue(r.Value.Permissions)!);
            var deniedRoles = user.Roles.Where(r => !(bool)perm.GetValue(r.Value.Permissions)!);

            var gRolesString = string.Join(", ", grantedRoles.Select(r => r.Value.Name));
            var dRolesString = string.Join(", ", deniedRoles.Select(r => r.Value.Name));

            if (!string.IsNullOrWhiteSpace(gRolesString)) embed.AddField("Roles that grant this permission", gRolesString);
            if (!string.IsNullOrWhiteSpace(dRolesString)) embed.AddField("Roles that don't have this permission", dRolesString);

            if (grantedRoles.Any())
            {
                embed.Description = $"To **deny** this permission, remove \"{permission}\" for the following roles: {gRolesString}.";
            }
            else
            {
                embed.Description = $"To **allow** this permission, allow \"{permission}\" for at least one of the following roles: {dRolesString}.";
            }

            return Ok(embed);
        }
    }
}