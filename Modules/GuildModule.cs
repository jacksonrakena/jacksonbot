using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Humanizer;
using Katbot.Attributes;
using Katbot.Checks.Command;
using Katbot.Entities;
using Katbot.Extensions;
using Katbot.Helpers;
using Katbot.Results;
using Qmmands;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Katbot.Modules
{
    [Name("Server Information")]
    [Description("Commands that help you interact with your server in useful and efficient ways.")]
    public class GuildModule : KatbotModuleBase
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

            var outStream = new MemoryStream();
            using (var image =
                SixLabors.ImageSharp.Image.Load(AssetHelper.GetAssetLocation("transparent_200x200.png")))
            {
                image.Mutate(a => a.BackgroundColor(new Rgba32(role.Color.R, role.Color.G, role.Color.B)));
                image.Save(outStream, new PngEncoder());
                outStream.Position = 0;
                await Context.Channel.SendFileAsync(outStream, "role.png", null, embed: new EmbedBuilder()
                    .WithColor(role.Color)
                    .WithTitle("Role Color")
                    .WithDescription(
                        $"**Hex:** {role.Color}\n\n**Red:** {role.Color.R}\n**Green:** {role.Color.G}\n**Blue:** {role.Color.B}")
                    .WithImageUrl("attachment://role.png")
                    .WithRequesterFooter(Context)
                    .WithCurrentTimestamp()
                    .Build()).ConfigureAwait(false);
            }

            return NoResult();
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
    }
}