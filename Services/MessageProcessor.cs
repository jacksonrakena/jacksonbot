using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Katbot.Entities;
using Katbot.Extensions;
using Katbot.Parsers;
using Katbot.Parsers.DiscordNet;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Katbot.Services
{
    public sealed class MessageProcessor : IMessageProcessor
    {
        // External objects
        private readonly DiscordSocketClient _client;

        // Domain objects
        private readonly KatbotConfig _config;

        // Type parsers
        private readonly DiscordEmoteTypeParser _emoteParser = new DiscordEmoteTypeParser();
        private readonly DiscordUserTypeParser _guildUserParser = new DiscordUserTypeParser();
        private readonly DiscordRoleTypeParser _socketRoleParser = new DiscordRoleTypeParser();
        private readonly BooleanTypeParser _boolTypeParser = new BooleanTypeParser();
        private readonly UriTypeParser _uriTypeParser = new UriTypeParser();

        // Services
        private readonly IServiceProvider _services;

        private readonly ICommandExecutor _commandExecutor;

        public MessageProcessor(DiscordSocketClient client, ICommandService commandService,
            IServiceProvider services, ILogger<MessageProcessor> logger,
            KatbotConfig config, ICommandExecutor commandExecutor)
        {
            _client = client;
            _services = services;
            _config = config;
            _commandExecutor = commandExecutor;
            
            commandService.AddTypeParser(_guildUserParser);
            commandService.AddTypeParser(_socketRoleParser);
            commandService.AddTypeParser(_emoteParser);
            commandService.AddTypeParser(_uriTypeParser);
            commandService.AddTypeParser(_boolTypeParser, true);
            _client.MessageReceived += ProcessMessageAsync;

            var modulesLoaded = commandService.AddModules(Assembly.GetEntryAssembly());

            logger.LogInformation(
                $"{modulesLoaded.Count} modules loaded, {modulesLoaded.Sum(a => a.Commands.Count)} commands loaded");
        }
        
        public async Task ProcessMessageAsync(SocketMessage incomingMessage)
        {
            if (!(incomingMessage is SocketUserMessage message) || message.Author is SocketWebhookUser
                || message.Author.IsBot)
            {
                return; // Ignore web-hooks or system messages
            }

            if (!(message.Channel is SocketGuildChannel))
            {
                await message.Channel.SendMessageAsync(
                        "Sorry, but I can only respond to commands in servers. Please try using your command in any of the servers that I share with you!")
                    .ConfigureAwait(false);
                return;
            }

            var argPos = 0;

            if (!await HasPrefixAsync(message, ref argPos).ConfigureAwait(false)) return;

            var context = new KatbotCommandContext(message, _services);

            await _commandExecutor.ExecuteAsync(context, message.Content.Substring(argPos)).ConfigureAwait(false);
        }

        public Task<bool> HasPrefixAsync(SocketUserMessage message, ref int argPos)
        {
            return Task.FromResult(
                message.HasStringPrefix(_config.CommandPrefix, ref argPos)
                || message.HasMentionPrefix(_client.CurrentUser, ref argPos));
        }
    }
}