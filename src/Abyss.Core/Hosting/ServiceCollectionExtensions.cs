using System;
using Microsoft.Extensions.DependencyInjection;
using Abyss.Core.Services;
using Abyss.Core.Entities;
using AbyssalSpotify;
using System.Net.Http;
using Discord.WebSocket;
using Discord;
using Qmmands;
using Abyss.Core;
using Abyss.Core.Parsers.UnixArguments;

namespace Abyss.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureSharedServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<BotService>();
            serviceCollection.AddSingleton<HelpService>();
            serviceCollection.AddSingleton<MessageReceiver>();
            serviceCollection.AddSingleton<ScriptingService>();
            serviceCollection.AddSingleton(provider =>
            {
                var configurationModel = provider.GetRequiredService<AbyssConfig>();
                return SpotifyClient.FromClientCredentials(configurationModel.Connections.Spotify.ClientId, configurationModel.Connections.Spotify.ClientSecret);
            });
            serviceCollection.AddTransient<Random>();
            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddSingleton<NotificationsService>();
            serviceCollection.AddSingleton<MarketingService>();

            serviceCollection.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Debug,
                DefaultRetryMode = RetryMode.RetryTimeouts,
                AlwaysDownloadUsers = true,
                ExclusiveBulkDelete = false
            }));

            var commandService = new CommandService(new CommandServiceConfiguration
            {
                StringComparison = StringComparison.OrdinalIgnoreCase,
                DefaultRunMode = RunMode.Sequential,
                IgnoresExtraArguments = true,
                CooldownBucketKeyGenerator = AbyssCooldownBucketKeyGenerators.Default,
                DefaultArgumentParser = DefaultArgumentParser.Instance
            });
            commandService.AddArgumentParser(UnixArgumentParser.Instance);
            serviceCollection.AddSingleton<ICommandService>(commandService);
        }
    }
}
