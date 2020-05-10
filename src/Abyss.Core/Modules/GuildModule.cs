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

        [Command("colour", "color")]
        [Description("Grabs the colour of a role.")]
        [RunMode(RunMode.Parallel)]
        public async Task Command_GetColourFromRoleAsync(
            [Name("Role")] [Description("The role you wish to view the colour of.")] [Remainder]
            CachedRole role)
        {
            if (role.Color == null || role.Color.Value == 0)
            {
                await ReplyAsync("That role doesn't have a colour.");
                return;
            }

            await using var outStream = ImageHelper.CreateColourImage(new Rgba32(role.Color.Value.R, role.Color.Value.G, role.Color.Value.B), 200, 200);
            await Context.Channel.SendMessageAsync(new LocalAttachment(outStream, "role.png"), null, embed: new LocalEmbedBuilder()
                .WithColor(role.Color)
                .WithTitle("Color")
                .WithDescription(
                    $"**Hex:** {role.Color}\n**Red:** {role.Color.Value.R}\n**Green:** {role.Color.Value.G}\n**Blue:** {role.Color.Value.B}")
                .WithImageUrl("attachment://role.png")
                .Build()).ConfigureAwait(false);
        }

        [Command("colour", "color")]
        [Description("Grabs the colour of a user.")]
        [RunMode(RunMode.Parallel)]
        public async Task Command_GetColourFromUserAsync(
            [Name("User")] [Description("The user you wish to view the colour of.")] [Remainder]
            CachedMember user)
        {
            var r = user.GetHighestRoleOrDefault(a => a.Color?.RawValue != 0);
            if (r == null)
            {
                await ReplyAsync("That user doesn't have a coloured role.");
                return;
            }
            await Command_GetColourFromRoleAsync(r);
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