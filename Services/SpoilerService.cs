using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Abyss.Entities;
using Abyss.Extensions;

namespace Abyss.Services
{
    public sealed class SpoilerService
    {
        private readonly DiscordSocketClient _client;

        private readonly Dictionary<ulong, (List<ulong> users, string name, string spoiler)> _spoilers =
            new Dictionary<ulong, (List<ulong> users, string name, string spoiler)>();

        public SpoilerService(DiscordSocketClient client)
        {
            _client = client;
            client.ReactionAdded += (cacheable, _, reaction) =>
            {
                Client_OnReactionAddedAsync(cacheable, reaction).ConfigureAwait(false);
                return Task.CompletedTask;
            };
            client.MessageDeleted += Client_OnMessageDeletedAsync;
        }

        private async Task Client_OnReactionAddedAsync(Cacheable<IUserMessage, ulong> messageCacheable,
            SocketReaction reaction)
        {
            var message = await messageCacheable.GetOrDownloadAsync().ConfigureAwait(false);

            if (_spoilers.TryGetValue(message.Id, out var spoiler))
            {
                if (spoiler.users.Contains(reaction.UserId)) return;
                try
                {
                    await _client.GetUser(reaction.UserId).SendMessageAsync(
                        $"**Spoiler for \"{spoiler.name}\":** " + spoiler.spoiler).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }

                spoiler.users.Add(reaction.UserId);
            }
        }

        private Task Client_OnMessageDeletedAsync(Cacheable<IMessage, ulong> messageCacheable,
            ISocketMessageChannel channel)
        {
            if (messageCacheable.HasValue && _spoilers.ContainsKey(messageCacheable.Value.Id))
                _spoilers.Remove(messageCacheable.Value.Id);

            return Task.CompletedTask;
        }

        public async Task CreateSpoilerMessageAsync(AbyssCommandContext context, string safe, string spoiler,
            string emote = "🛂")
        {
            await context.Message.DeleteAsync(new RequestOptions
            {
                AuditLogReason =
                    $"Abyss Spoiler Command - spoiler created by {context.Invoker} (ID {context.Invoker.Id})"
            }).ConfigureAwait(false);

            var embed = new EmbedBuilder();
            embed
                .WithTitle("Spoiler Created")
                .WithDescription(
                    $"**A spoiler has been created by {context.Invoker}!**\nThe spoiler's name is: \"{safe}\"\nClick on my Passport Control reaction emoji, and I will DM the spoiler's contents to you!")
                .WithColor(context.Invoker.GetHighestRoleColourOrDefault());

            var msg = await context.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

            await msg.AddReactionAsync(new Emoji(emote)).ConfigureAwait(false);

            _spoilers[msg.Id] = (new List<ulong>(), safe, spoiler);
        }
    }
}