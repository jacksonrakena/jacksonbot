using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    public class AbyssRequestContext : DiscordCommandContext, IServiceProvider
    {
        public AbyssRequestContext(AbyssBot bot, CachedUserMessage message, string prefix) : base(bot, prefix, message)
        {
            Bot = bot;

            Bot.Services.GetRequiredService<HelpService>();
            if (!Guild.Members.TryGetValue(Bot.CurrentUser.Id, out var botu)) throw new InvalidOperationException("Guild members doesn't contain current user.");
            BotMember = botu;
            Channel = (CachedTextChannel) message.Channel;
        }

        public new CachedTextChannel Channel { get; }
        public new AbyssBot Bot { get; }
        public CachedMember Invoker => this.Member;
        public CachedMember BotMember { get; }
        public bool InvokerIsOwner => Invoker.Id == Bot.CurrentApplication.GetOrDownloadAsync().GetAwaiter().GetResult().Owner.Id;

        public override string ToString() 
        {
            return
                $"{Command.Name} for {Invoker} (ID {Invoker.Id}) in #{Channel.Name} (ID {Channel.Id})/{Guild.Name} (ID {Guild.Id}) ";
        }

        public object GetService(Type serviceType) => ServiceProvider.GetService(serviceType);

        public Task<RestUserMessage?> ReplyAsync(string? content = null, EmbedBuilder? embed = null,
            RestRequestOptions? options = null)
        {
            if (!BotMember.GetPermissionsFor(Channel).SendMessages) return Task.FromResult((RestUserMessage?) null);
            return Channel.SendMessageAsync(content, false, embed?.Build(), options);
        }
    }
}