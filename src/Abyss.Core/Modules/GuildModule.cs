using Abyss.Core.Attributes;
using Abyss.Core.Checks.Command;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Helpers;
using Abyss.Core.Results;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Humanizer;
using Qmmands;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Abyss.Core.Modules
{
    [Name("Server Information")]
    [Description("Commands that help you interact with your server in useful and efficient ways.")]
    public class GuildModule : AbyssModuleBase
    {
        [Command("Nickname", "SetNick", "SetNickname", "Nick")]
        [Description("Sets the current nickname for a user.")]
        [Remarks("You can provide `clear` to remove their current nickname (if any).")]
        [Example("nickname @pyjamaclub BestPerson", "nick @pyjamaclub This Is The Best Person")]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireUserPermission(GuildPermission.ChangeNickname)]
        public async Task<ActionResult> Command_SetNicknameAsync(
            [Name("Target")] [Description("The user you would like me to change username of.")] SocketGuildUser target,
            [Description("The nickname to set to. `clear` to remove one (if set).")] [Name("Nickname")] [Remainder]
            string nickname)
        {
            if (nickname.Equals("clear", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(target.Nickname))
                return BadRequest($"{target.Format()} doesn't have a nickname!");

            try
            {
                await target.ModifyAsync(a => a.Nickname = nickname == "clear" ? null : nickname, new RequestOptions
                {
                    AuditLogReason = $"Action performed by {Context.Invoker}"
                }).ConfigureAwait(false);
                return Ok(nickname != "clear"
                    ? $"Set {target.Format()}'s nickname to `" + nickname + "`."
                    : "Done!");
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
            {
                return BadRequest("Received 403 Forbidden changing nickname!");
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.BadRequest)
            {
                return BadRequest("Received 400 Bad Request, try shortening or extending the name!");
            }
        }

        [Command("Server", "ServerInfo", "Guild", "GuildInfo")]
        [Description("Grabs information around this server.")]
        [Example("server")]
        public Task<ActionResult> Command_GuildInfoAsync()
        {
            var embed = new EmbedBuilder
            {
                Color = BotService.DefaultEmbedColour,
                Author = new EmbedAuthorBuilder
                {
                    Name = "Information for server " + Context.Guild.Name,
                    IconUrl = Context.Guild.IconUrl
                },
                ThumbnailUrl = Context.Guild.IconUrl,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Owner",
                        Value = Context.Guild.Owner.ToString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Created At",
                        Value = UserModule.FormatOffset(Context.Guild.CreatedAt),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Channels",
                        Value = string.Join(", ", Context.Guild.Channels.Select(c => c.Name)) + " (" +
                                Context.Guild.Channels.Count + ")",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Voice Channels",
                        Value = string.Join(", ", Context.Guild.VoiceChannels.Select(vc => vc.Name)) + " (" +
                                Context.Guild.VoiceChannels.Count + ")",
                        IsInline = true
                    }
                }
            };

            return Ok(embed);
        }

        [Command("Colour", "Color")]
        [Description("Grabs the colour of a role.")]
        [RunMode(RunMode.Parallel)]
        [Example("color Admin", "color @Owner", "color 413956873256042496")]
        public async Task<ActionResult> Command_GetColourFromRoleAsync(
            [Name("Role")] [Description("The role you wish to view the colour of.")] [Remainder]
            SocketRole role)
        {
            if (role.Color.RawValue == 0) return BadRequest("That role does not have a colour!");

            var outStream = ImageHelper.CreateColourImage(new Rgba32(role.Color.R, role.Color.G, role.Color.B));
            await Context.Channel.SendFileAsync(outStream, "role.png", null, embed: new EmbedBuilder()
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

        [Command("Colour", "Color")]
        [Description("Grabs the colour of a user.")]
        [Example("color pyjamaclub", "color @OtherUser", "color 413956873256042496")]
        [RunMode(RunMode.Parallel)]
        public Task<ActionResult> Command_GetColourFromUserAsync(
            [Name("User")] [Description("The user you wish to view the colour of.")] [Remainder]
            SocketGuildUser user)
        {
            var r = user.GetHighestRoleOrDefault(a => a.Color.RawValue != 0);
            return r == null
                ? BadRequest("That user does not have a coloured role!")
                : Command_GetColourFromRoleAsync((SocketRole) r);
        }

        [Command("Permissions", "Perms")]
        [Description("Shows a list of a user's current guild-level permissions.")]
        [Example("permissions pyjamaclub", "perms @OtherUser", "perms 413956873256042496")]
        public Task<ActionResult> Command_ShowPermissionsAsync(
            [Name("Target")]
            [Description("The user to get permissions for.")]
            [DefaultValueDescription("The user who invoked this command.")]
            SocketGuildUser user = null)
        {
            user = user ?? Context.Invoker; // Get the user (or the invoker, if none specified)

            var embed = new EmbedBuilder();
            embed.WithAuthor((IUser) user);

            if (user.Id == Context.Guild.OwnerId)
            {
                embed.WithDescription("User is owner of server, and has all permissions");
                return Ok(embed);
            }

            if (user.GuildPermissions.Administrator)
            {
                embed.WithDescription("User has Administrator permission, and has all permissions");
                return Ok(embed);
            }

            var guildPerms = user.GuildPermissions; // Get the user's permissions

            var booleanTypeProperties = guildPerms.GetType().GetProperties()
                .Where(a => a.PropertyType.IsAssignableFrom(typeof(bool)))
                .ToList(); // Get all properties that have a property type of Boolean

            var propDict = booleanTypeProperties.Select(a => (a.Name.Humanize(), (bool) a.GetValue(guildPerms)))
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

        [Command("HasPerm")]
        [Description("Checks if I have a permission accepted.")]
        [Example("hasperm Manage Messages", "hasperm Ban Members")]
        public Task<ActionResult> Command_HasPermissionAsync(
            [Name("Permission")] [Remainder] [Description("The permission to check for.")]
            string permission)
        {
            var guildPerms = Context.Guild.CurrentUser.GuildPermissions;
            var props = guildPerms.GetType().GetProperties();

            var boolProps = props.Where(a =>
                a.PropertyType.IsAssignableFrom(typeof(bool))
                && (a.Name.Equals(permission, StringComparison.OrdinalIgnoreCase)
                 || a.Name.Humanize().Equals(permission, StringComparison.OrdinalIgnoreCase))).ToList();
            /* Get a list of all properties of Boolean type and that match either the permission specified, or match it   when humanized */

            if (boolProps.Count == 0) return BadRequest($"Unknown permission `{permission}` :(");

            var perm = boolProps[0];
            var name = perm.Name.Humanize();
            var value = (bool) perm.GetValue(guildPerms);

            return Ok(a => a.WithDescription($"I **{(value ? "do" : "do not")}** have permission `{name}`!"));
        }

        [Command("AnalyzePermission", "AnalyzePerm", "Analyze")]
        [Description("Analyzes a permission for a user, and sees which role grants or denies that permission to them.")]
        [Example("analyzeperm Abyssal Manage Messages")]
        public Task<ActionResult> Command_AnalyzePermissionAsync(
            [Name("User")] [Description("The user to check for the specified permission.")]
            SocketGuildUser user,
            [Name("Permission")] [Description("The permission to analyze.")] [Remainder] string permission)
        {
            user = user ?? Context.Invoker;

            var perm = user.GuildPermissions.GetType().GetProperties().FirstOrDefault(a =>
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

            var p = user.Roles.Where(a => a.Permissions.Administrator).ToList();
            if (p.Count > 0)
            {
                embed.Description = 
                    "User has administrator, and has every permission. To deny them administrator, remove the Administrator permission from the following roles:\n" +
                    $"`{string.Join(", ", p.Select(b => b.Name))}`";
                return Ok(embed);
            }

            var grantedRoles = user.Roles.Where(r => (bool) perm.GetValue(r.Permissions));
            var deniedRoles = user.Roles.Where(r => !((bool) perm.GetValue(r.Permissions)));

            var gRolesString = string.Join(", ", grantedRoles.Select(r => r.Name));
            var dRolesString = string.Join(", ", deniedRoles.Select(r => r.Name));

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