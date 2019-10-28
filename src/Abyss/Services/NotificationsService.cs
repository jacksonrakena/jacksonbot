using Disqord;
using System;
using System.Linq;
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
        }

        public async Task NotifyReadyAsync(bool firstTime = false)
        {
            if (_notifyConfig?.Ready == null) return;
            var ch = _abyss.GetChannel(_notifyConfig.Ready.Value);
            if (!(ch != null && ch is CachedTextChannel stc)) return;


            var embed = new EmbedBuilder()
                .WithAuthor(_abyss.CurrentUser.ToEmbedAuthorBuilder())
                .WithColor(AbyssHostedService.DefaultEmbedColour)
                .WithCurrentTimestamp()
                .AddField("Core version", Assembly.GetExecutingAssembly().GetName().Version!.ToString(), true)
                .AddField("Guilds", _abyss.Guilds.Count, true)
                .AddField("Loaded assemblies", string.Join(", ", _abyss.LoadedAssemblies.Select(c => $"{c.GetName().Name} (v{c.GetName().Version})")))
                .WithThumbnailUrl(_abyss.CurrentUser.GetAvatarUrl());

            await stc.SendMessageAsync(null, false, embed
                .WithDescription($"Abyss instance {(firstTime ? "started and" : "")} ready at {DateTime.Now:F}.")
                .Build());
        }

        public async Task NotifyStoppingAsync()
        {
            if (_notifyConfig?.Stopping == null) return;

            var ch = _abyss.GetChannel(_notifyConfig.Stopping.Value);
            if (!(ch != null && ch is CachedTextChannel stc)) return;

            await stc.SendMessageAsync(null, false, new EmbedBuilder()
                    .WithAuthor(_abyss.CurrentUser.ToEmbedAuthorBuilder())
                    .WithDescription($"Abyss instance stopping at " + DateTime.Now.ToString("F"))
                    .WithColor(AbyssHostedService.DefaultEmbedColour)
                    .WithCurrentTimestamp()
                    .WithThumbnailUrl(_abyss.CurrentUser.GetAvatarUrl())
                    .Build());
            return;
        }

        public async Task NotifyServerMembershipChangeAsync(CachedGuild arg, bool botIsJoining)
        {
            if (_notifyConfig?.ServerMembershipChange == null) return;
            var updateChannel = _abyss.GetChannel(_notifyConfig.ServerMembershipChange.Value);
            if (!(updateChannel is CachedTextChannel stc)) return;
            await stc.SendMessageAsync(null, false, new EmbedBuilder()
                 .WithAuthor(_abyss.CurrentUser.ToEmbedAuthorBuilder())
                 .WithDescription($"{(botIsJoining ? "Joined" : "Left")} {arg.Name} at {DateTime.Now:F}")
                 .AddField("Member count", arg.MemberCount, true)
                 .AddField("Channel count", arg.TextChannels.Count + " text / " + arg.VoiceChannels.Count + " voice", true)
                 .AddField("Owner", $"{arg.Owner} ({arg.OwnerId})", true)
                 .AddField("Total bot guilds", _abyss.Guilds.Count, true)
                 .WithColor(AbyssHostedService.DefaultEmbedColour)
                 .WithThumbnailUrl(arg.GetIconUrl())
                 .WithCurrentTimestamp()
                 .Build());
        }
    }
}
