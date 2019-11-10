using Disqord;
using Humanizer;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Abyss
{
    public class NotificationsService
    {
        private readonly AbyssConfigNotificationsSection? _notifyConfig;
        private readonly AbyssConfigEmoteSection _emotes;
        private readonly AbyssBot _abyss;
        private string CurrentDateTime => Markdown.Code("[" + DateTime.Now.ToString("HH:mm:ss yyyy-MM-dd") + "]");

        public NotificationsService(AbyssBot abyss, AbyssConfig config)
        {
            _notifyConfig = config.Notifications;
            _emotes = config.Emotes;
            _abyss = abyss;
        }

        public async Task NotifyReadyAsync(bool firstTime = false)
        {
            if (_notifyConfig?.Ready == null) return;
            var ch = _abyss.GetChannel(_notifyConfig.Ready.Value);
            if (!(ch != null && ch is CachedTextChannel stc)) return;

            var message = $"{CurrentDateTime} {_emotes.OnlineEmote} **{_abyss.CurrentUser.Name}** is now online. " +
            $"Connected to {_abyss.Guilds.Count} servers. {_abyss.LoadedPacks.Count} packs loaded: {_abyss.LoadedPacks.Select(c => $"{c.FriendlyName} (v{c.Assembly.GetVersion()})").Humanize()}";

            await stc.SendMessageAsync(message);
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
