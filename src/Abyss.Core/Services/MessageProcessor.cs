using Abyss.Entities;
using Abyss.Extensions;
using Abyss.Parsers;
using Abyss.Parsers.DiscordNet;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public sealed class MessageProcessor : IMessageProcessor
    {
        // External objects
        private readonly DiscordSocketClient _client;

        // Domain objects
        private readonly AbyssConfig _config;

        // Type parsers
        private readonly DiscordEmoteTypeParser _emoteParser = new DiscordEmoteTypeParser();

        private readonly DiscordUserTypeParser _guildUserParser = new DiscordUserTypeParser();
        private readonly DiscordRoleTypeParser _socketRoleParser = new DiscordRoleTypeParser();
        private readonly BooleanTypeParser _boolTypeParser = new BooleanTypeParser();
        private readonly UriTypeParser _uriTypeParser = new UriTypeParser();

        // Services
        private readonly IServiceProvider _services;

        private readonly ICommandService _commandService;

        private readonly ICommandExecutor _commandExecutor;

        private readonly ILogger<MessageProcessor> _logger;

        public MessageProcessor(DiscordSocketClient client, ICommandService commandService,
            IServiceProvider services, ILogger<MessageProcessor> logger,
            AbyssConfig config, ICommandExecutor commandExecutor)
        {
            _client = client;
            _services = services;
            _config = config;
            _commandService = commandService;
            _commandExecutor = commandExecutor;
            _logger = logger;

            commandService.AddTypeParser(_guildUserParser);
            commandService.AddTypeParser(_socketRoleParser);
            commandService.AddTypeParser(_emoteParser);
            commandService.AddTypeParser(_uriTypeParser);
            commandService.AddTypeParser(_boolTypeParser, true);
            _client.MessageReceived += ProcessMessageAsync;

            LoadModulesFromAssembly(Assembly.GetExecutingAssembly());

            var assembliesToLoad = new List<Assembly> { Assembly.GetExecutingAssembly() };
            var assemblyDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomAssemblies");
        }

        public void LoadModulesFromAssembly(Assembly assembly)
        {
            var rootModulesLoaded = _commandService.AddModules(assembly);

            _logger.LogInformation(
                $"{rootModulesLoaded.Count} modules loaded, {rootModulesLoaded.Sum(a => a.Commands.Count)} commands loaded, from assembly {assembly.GetName().Name}");
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

            var context = new AbyssCommandContext(message, _services);

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