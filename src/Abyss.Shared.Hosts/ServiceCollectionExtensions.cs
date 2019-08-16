using System;
using System.Net.Http;
using Abyss.Core;
using Abyss.Core.Addons;
using Abyss.Core.Entities;
using Abyss.Core.Services;
using AbyssalSpotify;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Abyss.Shared.Hosts
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
                return SpotifyClient.FromClientCredentials(configurationModel.Connections.Spotify.ClientId,
                    configurationModel.Connections.Spotify.ClientSecret);
            });
            serviceCollection.AddTransient<Random>();
            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddSingleton<ResponseCacheService>();
            serviceCollection.AddSingleton<AddonService>();
            serviceCollection.AddSingleton<NotificationsService>();
            serviceCollection.AddSingleton<DataService>();
            serviceCollection.AddSingleton<MarketingService>();

            serviceCollection.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Debug,
                DefaultRetryMode = RetryMode.RetryTimeouts,
                AlwaysDownloadUsers = true
            }));

            serviceCollection.AddSingleton<ICommandService>(new CommandService(new CommandServiceConfiguration
            {
                StringComparison = StringComparison.OrdinalIgnoreCase,
                DefaultRunMode = RunMode.Sequential,
                IgnoresExtraArguments = true,
                CooldownBucketKeyGenerator = AbyssCooldownBucketKeyGenerators.Default
            }));
        }
    }
}