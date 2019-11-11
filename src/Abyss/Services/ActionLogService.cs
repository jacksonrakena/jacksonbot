using Disqord;
using Disqord.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abyss
{
    public class ActionLogService
    {
        private readonly AbyssBot _bot;
        private readonly DatabaseService _database;
        private readonly AbyssConfigEmoteSection _emotes;

        public ActionLogService(AbyssBot bot, DatabaseService database, AbyssConfig config)
        {
            _bot = bot;
            _database = database;
            _emotes = config.Emotes;

            _bot.ChannelCreated += ChannelCreated;
            _bot.ChannelUpdated += ChannelUpdated;
            _bot.ChannelDeleted += ChannelDeleted;
            _bot.ChannelPinsUpdated += ChannelPinsUpdated;
            _bot.GuildUpdated += GuildUpdated;
            _bot.MemberBanned += MemberBanned;
            _bot.MemberJoined += MemberJoined;
            _bot.MemberLeft += MemberLeft;
            _bot.MemberUnbanned += MemberUnbanned;
            _bot.MemberUpdated += MemberUpdated;
            _bot.MessageDeleted += MessageDeleted;
            _bot.MessagesBulkDeleted += MessagesBulkDeleted;
            _bot.MessageUpdated += MessageUpdated;
            _bot.ReactionsCleared += ReactionsCleared;
            _bot.RoleCreated += RoleCreated;
            _bot.RoleDeleted += RoleDeleted;
            _bot.RoleUpdated += RoleUpdated;
        }

        public Task<bool> CreateModeratorActionLogEntryAsync(string message, ulong guildId)
        {
            return CreateActionLogEntryAsync(message, _emotes.StaffEmote, guildId);
        }

        private async Task<bool> CreateActionLogEntryAsync(string message, string emoji, ulong guildId)
        {
            var guildData = _database.GetOrCreateGuild(guildId);
            if (guildData.ActionLogChannelId == 0) return false;
            if (!(_bot.GetGuildChannel(guildData.ActionLogChannelId) is CachedTextChannel guildChannel)) return false;
            return await guildChannel.TrySendMessageAsync(
                $"{Markdown.Code("[" + DateTime.Now.ToUniversalTime().ToString("HH:mm:ss yyyy-MM-dd") + "]")} {emoji} {message}");
        }

        private Task ChannelCreated(ChannelCreatedEventArgs e)
        {
            if (!(e.Channel is CachedGuildChannel cgc)) return Task.CompletedTask;
            return CreateActionLogEntryAsync($"Channel `{cgc.Name}` ({cgc.Id}) was created.", _emotes.GuildOwnerEmote, cgc.Guild.Id); ;
        }

        private Task ChannelUpdated(ChannelUpdatedEventArgs e)
        {
            if (!(e.NewChannel is CachedGuildChannel cgc)) return Task.CompletedTask;
            return CreateActionLogEntryAsync($"Channel `{cgc.Name}` ({cgc.Id}) was updated.{(e.OldChannel.Name != e.NewChannel.Name ? $" The name was updated from `{e.OldChannel.Name}` to `{e.NewChannel.Name}`." : "")}", _emotes.GuildOwnerEmote, cgc.Guild.Id);
        }

        private Task ChannelDeleted(ChannelDeletedEventArgs e)
        {
            if (!(e.Channel is CachedGuildChannel cgc)) return Task.CompletedTask;
            return CreateActionLogEntryAsync($"Channel `{cgc.Name}` ({cgc.Id}) was deleted.", _emotes.GuildOwnerEmote, cgc.Guild.Id);
        }

        private Task ChannelPinsUpdated(ChannelPinsUpdatedEventArgs e)
        {
            // TO-DO
            return Task.CompletedTask;
        }

        private Task GuildUpdated(GuildUpdatedEventArgs e)
        {
            return CreateActionLogEntryAsync($"This server was updated.{(e.OldGuild.Name != e.NewGuild.Name ? $" The name was updated from `{e.OldGuild.Name}` to `{e.NewGuild.Name}`." : "")}", _emotes.YesEmote, e.NewGuild.Id);
        }

        private Task MemberBanned(MemberBannedEventArgs e)
        {
            return CreateActionLogEntryAsync($"Member `{e.User}` ({e.User.Id}) was banned.", _emotes.YesEmote, e.Guild.Id);
        }

        private Task MemberJoined(MemberJoinedEventArgs e)
        {
            return CreateActionLogEntryAsync($"Member `{e.Member}` ({e.Member.Id}) joined.", _emotes.OnlineEmote, e.Member.Guild.Id);
        }

        private Task RoleUpdated(RoleUpdatedEventArgs e)
        {
            return CreateActionLogEntryAsync($"Role `{e.NewRole.Name}` ({e.NewRole.Id}) was updated.{(e.OldRole.Name != e.NewRole.Name ? $" The name was updated from `{e.OldRole.Name}` to `{e.NewRole.Name}`." : "")}", _emotes.YesEmote, e.NewRole.Guild.Id);
        }

        private Task RoleDeleted(RoleDeletedEventArgs e)
        {
            return CreateActionLogEntryAsync($"Role `{e.Role.Name}` ({e.Role.Id}) was deleted.", _emotes.YesEmote, e.Role.Guild.Id);
        }

        private Task RoleCreated(RoleCreatedEventArgs e)
        {
            return CreateActionLogEntryAsync($"Role `{e.Role.Name}` ({e.Role.Id}) was created.", _emotes.YesEmote, e.Role.Guild.Id);
        }

        private Task ReactionsCleared(ReactionsClearedEventArgs e)
        {
            if (!e.Message.HasValue || !(e.Message.Value.Channel is CachedTextChannel ctc)) return Task.CompletedTask;
            return CreateActionLogEntryAsync($"All reactions were cleared from message {e.Message.Value.Id} (author {e.Message.Value.Author}), in channel {e.Message.Value.Channel.Name}.", _emotes.YesEmote, ctc.Guild.Id);
        }

        private Task MessageUpdated(MessageUpdatedEventArgs e)
        {
            if (!(e.Channel is CachedTextChannel ctc)) return Task.CompletedTask;
            var firstString = new StringBuilder($"A message ({e.NewMessage.Id}) from {e.NewMessage.Author} ({e.NewMessage.Author.Id}) was updated. ");
            if (e.OldMessage.HasValue)
            {
                firstString.Append($"The content was changed from \"{e.OldMessage.Value.Content}\" to \"{e.NewMessage.Content}\".");
            }
            return CreateActionLogEntryAsync(firstString.ToString(), _emotes.YesEmote, ctc.Guild.Id);
        }

        private Task MessagesBulkDeleted(MessagesBulkDeletedEventArgs e)
        {
            return CreateActionLogEntryAsync($"{e.Messages.Count} messages were bulk deleted in {e.Channel.Mention}.", _emotes.StaffEmote, e.Channel.Guild.Id);
        }

        private Task MessageDeleted(MessageDeletedEventArgs e)
        {
            if (!e.Message.HasValue || !(e.Channel is CachedTextChannel ctc)) return Task.CompletedTask;
            return CreateActionLogEntryAsync($"A message ({e.Message.Value.Id}) from {e.Message.Value.Author} in channel {ctc.Mention} was deleted.{(e.Message.Value.Content != null ? $" Message content: \"{e.Message.Value.Content}\"." : "")}", _emotes.StaffEmote, ctc.Guild.Id);
        }

        private Task MemberUpdated(MemberUpdatedEventArgs e)
        {
            if (e.OldMember != null && e.NewMember.Nick != e.OldMember.Nick)
            {
                return CreateActionLogEntryAsync($"The nickname of `{e.NewMember}` ({e.NewMember.Id}) was updated from `{e.OldMember.Nick ?? "None"}` to `{e.NewMember.Nick ?? "None"}`", _emotes.StaffEmote, e.NewMember.Guild.Id);
            }
            else return CreateActionLogEntryAsync($"Member `{e.NewMember}` ({e.NewMember.Id}) was updated.", _emotes.StaffEmote, e.NewMember.Guild.Id);
        }

        private Task MemberUnbanned(MemberUnbannedEventArgs e)
        {
            return CreateActionLogEntryAsync($"User `{e.User}` ({e.User.Id}) was unbanned.", _emotes.YesEmote, e.Guild.Id);
        }

        private Task MemberLeft(MemberLeftEventArgs e)
        {
            return CreateActionLogEntryAsync($"User `{e.User}` ({e.User.Id}) left.", _emotes.YesEmote, e.Guild.Id);
        }
    }
}