using Disqord;
using Disqord.Events;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    public class NotificationsService
    {
        private readonly AbyssConfigNotificationsSection? _notifyConfig;
        private readonly AbyssConfigEmoteSection _emotes;
        private readonly AbyssBot _abyss;
        private string CurrentDateTime => Markdown.Code("[" + DateTime.Now.ToUniversalTime().ToString("HH:mm:ss yyyy-MM-dd") + "]");

        public NotificationsService(AbyssBot abyss, AbyssConfig config)
        {
            _notifyConfig = config.Notifications;
            _emotes = config.Emotes;
            _abyss = abyss;

            _abyss.Ready += NotifyReadyAsync;
            _abyss.JoinedGuild += GuildJoined;
            _abyss.LeftGuild += GuildLeft;
        }

        public async Task NotifyReadyAsync(ReadyEventArgs args)
        {
            if (_notifyConfig?.Ready == null) return;
            var ch = _abyss.GetChannel(_notifyConfig.Ready.Value);
            if (!(ch != null && ch is CachedTextChannel stc)) return;

            var message = $"{CurrentDateTime} {_emotes.OnlineEmote} **{_abyss.CurrentUser.Name}** is ready. " +
            $"Connected to {_abyss.Guilds.Count} servers.";

            await stc.SendMessageAsync(message).ConfigureAwait(false);
        }

        public async Task NotifyStoppingAsync()
        {
            if (_notifyConfig?.Stopping == null) return;

            var ch = _abyss.GetChannel(_notifyConfig.Stopping.Value);
            if (!(ch != null && ch is CachedTextChannel stc)) return;

            var message = $"{CurrentDateTime} {_emotes.OfflineEmote} **{_abyss.CurrentUser.Name}** is stopping.";

            await stc.SendMessageAsync(message);
            return;
        }

        private Task GuildJoined(JoinedGuildEventArgs e) => NotifyServerMembershipChangeAsync(e.Guild, true);
        private Task GuildLeft(LeftGuildEventArgs e) => NotifyServerMembershipChangeAsync(e.Guild, false);

        public async Task NotifyServerMembershipChangeAsync(CachedGuild arg, bool botIsJoining)
        {
            if (_notifyConfig?.ServerMembershipChange == null) return;
            var updateChannel = _abyss.GetChannel(_notifyConfig.ServerMembershipChange.Value);
            if (!(updateChannel is CachedTextChannel stc)) return;

            var message = $"{CurrentDateTime} {_emotes.GuildOwnerEmote} **{_abyss.CurrentUser.Name}** has {(botIsJoining ? "joined" : "left")} {Markdown.Code(arg.Name)}, with " +
            $"{arg.MemberCount} members and owned by {arg.Owner?.ToString() ?? arg.OwnerId.ToString()}. Connected to {_abyss.Guilds.Count} servers total.";

            await stc.SendMessageAsync(message);
        }
    }
}
