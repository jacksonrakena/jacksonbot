using Disqord;
using Disqord.Events;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Abyss
{
    public class NotificationsService
    {
        private readonly AbyssConfigNotificationsSection? _notifyConfig;
        private readonly AbyssBot _abyss;

        public NotificationsService(AbyssBot abyss, AbyssConfig config)
        {
            _notifyConfig = config.Notifications;
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

            var message = new LocalEmbedBuilder()
                .WithColor(stc.Guild.CurrentMember.GetHighestRoleColourOrSystem())
                .WithCurrentTimestamp()
                .WithAuthor(args.Client.CurrentUser)
                .WithDescription("Ready.")
                .AddField("Servers", args.Client.Guilds.Count, true)
                .AddField("Version", Assembly.GetExecutingAssembly().GetVersion(), true)
                .AddField("Time UTC", FormatHelper.FormatTime(DateTimeOffset.Now), true)
                .AddField("Session ID", args.SessionId);

            await stc.TrySendMessageAsync(embed: message.Build()).ConfigureAwait(false);
        }

        /*public Task NotifyStoppingAsync()
        {
            if (_notifyConfig?.Stopping == null) return Task.CompletedTask;

            var ch = _abyss.GetChannel(_notifyConfig.Stopping.Value);
            if (!(ch != null && ch is CachedTextChannel stc)) return Task.CompletedTask;

            var message = new LocalEmbedBuilder()
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .WithAuthor(_abyss.CurrentUser)
                .WithDescription("Stopping.")
                .AddField("Time UTC", FormatHelper.FormatTime(DateTimeOffset.Now), true);

            return stc.TrySendMessageAsync(embed: message.Build());
        }*/

        private Task GuildJoined(JoinedGuildEventArgs e) => NotifyServerMembershipChangeAsync(e.Guild, true);
        private Task GuildLeft(LeftGuildEventArgs e) => NotifyServerMembershipChangeAsync(e.Guild, false);

        public async Task NotifyServerMembershipChangeAsync(CachedGuild arg, bool botIsJoining)
        {
            if (_notifyConfig?.ServerMembershipChange == null) return;
            var updateChannel = _abyss.GetChannel(_notifyConfig.ServerMembershipChange.Value);
            if (!(updateChannel is CachedTextChannel stc)) return;

            var message = new LocalEmbedBuilder()
                .WithColor(botIsJoining ? Color.Green : Color.Yellow)
                .WithCurrentTimestamp()
                .WithAuthor($"{(botIsJoining ? "Joined" : "Left")} server")
                .AddField("Name", arg.Name, true)
                .AddField("Members", arg.MemberCount, true)
                .AddField("Owner", arg.Owner.ToString(), true);

            await stc.TrySendMessageAsync(embed: message.Build()).ConfigureAwait(false);
        }
    }
}
