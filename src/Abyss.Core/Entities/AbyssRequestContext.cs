using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Sentry;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Entities
{
    public class AbyssRequestContext : CommandContext
    {
        public AbyssRequestContext(SocketUserMessage message, IServiceProvider services)
        {
            Message = message;
            Channel = (SocketTextChannel) message.Channel;
            Services = services;
            Client = Services.GetRequiredService<DiscordSocketClient>();
            Guild = Channel.Guild;
            Bot = Client.CurrentUser;
            Invoker = (SocketGuildUser) message.Author;
            BotUser = Guild.GetUser(Bot.Id);
        }

        public IDisposable RequestScopeHandle { get; set; }
        public SocketGuildUser Invoker { get; }
        public SocketSelfUser Bot { get; }
        public SocketGuildUser BotUser { get; }
        public DiscordSocketClient Client { get; }
        public IServiceProvider Services { get; }
        public SocketUserMessage Message { get; }
        public SocketTextChannel Channel { get; }
        public SocketGuild Guild { get; }
        public bool InvokerIsOwner => Invoker.Id == Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id;

        public string FormatString()
        {
            return
                $"Executing {Command.Name} for {Invoker} (ID {Invoker.Id}) in #{Channel.Name} (ID {Channel.Id})/{Guild.Name} (ID {Guild.Id}) ";
        }

        public string GetPrefix()
        {
            return Services.GetRequiredService<AbyssConfig>().CommandPrefix;
        }

        public Task<RestUserMessage> ReplyAsync(string content = null, EmbedBuilder embed = null,
            RequestOptions options = null)
        {
            if (!BotUser.GetPermissions(Channel).SendMessages) return Task.FromResult((RestUserMessage) null);
            return Channel.SendMessageAsync(content, false, embed?.Build(), options);
        }
    }
}