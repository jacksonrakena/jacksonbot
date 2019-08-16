using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Services
{
    public class NotificationsService
    {
        private readonly bool _sendNotifications = true;
        private readonly AbyssConfigNotificationsSection _notifyConfig;
        private readonly IHostEnvironment _environment;
        private readonly DiscordSocketClient _client;

        public NotificationsService(AbyssConfig config, DiscordSocketClient client, IHostEnvironment env)
        {
            if (config.Notifications == null) _sendNotifications = false;
            _notifyConfig = config.Notifications;
            _client = client;
            _environment = env;
        }

        public async Task NotifyReadyAsync(bool firstTime = false)
        {
            if (!_sendNotifications || _notifyConfig.Ready == null) return;
            var ch = _client.GetChannel(_notifyConfig.Ready.Value);
            if (!(ch != null && ch is SocketTextChannel stc)) return;

            if (firstTime)
            {
                await stc.SendMessageAsync(null, false, new EmbedBuilder()
                    .WithAuthor(_client.CurrentUser.ToEmbedAuthorBuilder())
                    .WithDescription($"{_environment.ApplicationName} instance started and ready at " + DateTime.Now.ToString("F"))
                    .WithColor(BotService.DefaultEmbedColour)
                    .WithCurrentTimestamp()
                    .WithThumbnailUrl(_client.CurrentUser.GetEffectiveAvatarUrl(2048))
                    .Build());
                return;
            }

            await stc.SendMessageAsync(null, false, new EmbedBuilder()
                .WithAuthor(_client.CurrentUser.ToEmbedAuthorBuilder())
                .WithDescription("Ready at " + DateTime.Now.ToString("F"))
                .WithColor(BotService.DefaultEmbedColour)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(_client.CurrentUser.GetEffectiveAvatarUrl(2048))
                .Build());
        }

        public async Task NotifyStoppingAsync()
        {
            if (!_sendNotifications || _notifyConfig.Stopping == null) return;

            var ch = _client.GetChannel(_notifyConfig.Stopping.Value);
            if (!(ch != null && ch is SocketTextChannel stc)) return;

            await stc.SendMessageAsync(null, false, new EmbedBuilder()
                    .WithAuthor(_client.CurrentUser.ToEmbedAuthorBuilder())
                    .WithDescription($"{_environment.ApplicationName} instance stopping at " + DateTime.Now.ToString("F"))
                    .WithColor(BotService.DefaultEmbedColour)
                    .WithCurrentTimestamp()
                    .WithThumbnailUrl(_client.CurrentUser.GetEffectiveAvatarUrl(2048))
                    .Build());
            return;
        }

        public async Task NotifyServerMembershipChangeAsync(SocketGuild arg, bool botIsJoining)
        {
            if (!_sendNotifications || _notifyConfig.ServerMembershipChange == null) return;
            var updateChannel = _client.GetChannel(_notifyConfig.ServerMembershipChange.Value);
            if (!(updateChannel is SocketTextChannel stc)) return;
             await stc.SendMessageAsync(null, false, new EmbedBuilder()
                 .WithAuthor(_client.CurrentUser.ToEmbedAuthorBuilder())
                 .WithDescription($"{(botIsJoining ? "Joined" : "Left")} {arg.Name} at {DateTime.Now:F} ({arg.MemberCount} members, owner: {arg.Owner})")
                 .WithColor(BotService.DefaultEmbedColour)
                 .WithThumbnailUrl(arg.IconUrl)
                 .WithCurrentTimestamp()
                 .Build());
        }

        public async Task NotifyExceptionAsync(Exception exception)
        {
            if (!_sendNotifications || _notifyConfig.Exception == null) return;
            var notifChannel = _client.GetChannel(_notifyConfig.Exception.Value);
            if (!(notifChannel is SocketTextChannel stc)) return;
            await stc.TrySendMessageAsync(null, false, new EmbedBuilder()
                .WithAuthor("Exception thrown", _client.CurrentUser.GetEffectiveAvatarUrl())
                .WithDescription($"Exception of type \"{exception.GetType().Name}\" thrown.")
                .AddField("Message", exception.Message)
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build());
        }
    }
}
