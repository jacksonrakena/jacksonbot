using Abyss.Addons;
using Abyss.Core.Services;
using Abyss.Entities;
using Abyss.Extensions;
using Abyss.Services;
using AbyssalSpotify;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Abyss.Console
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            System.Console.WriteLine("Abyss console host application starting at " + DateTime.Now.ToString("F"));

            var services = ConfigureServices(args);

            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Abyss Console Host");
            logger.LogInformation($"Took {(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds} seconds to initialize services. Console host starting.");

            var bot = services.GetRequiredService<BotService>();

            await bot.StartAsync(CancellationToken.None).ConfigureAwait(false);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static IServiceProvider ConfigureServices(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            // Bot core
            serviceCollection.AddSingleton<BotService>();

            // Configuration
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(serviceProvider.GetRequiredService<DataService>().GetConfigurationBasePath())
                .AddJsonFile("Abyss.json", false, true);
                return configurationBuilder.Build();
            });
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var configurationModel = new AbyssConfig();
                serviceProvider.GetRequiredService<IConfigurationRoot>().Bind(configurationModel);

                return configurationModel;
            });

            // Logging
            serviceCollection.AddLogging(builder => builder.AddAbyss());

            // Discord
            var (discordClient, discordConfig) = ConfigureDiscord();
            serviceCollection.AddSingleton(discordClient);
            serviceCollection.AddSingleton(discordConfig);

            // Commands
            var (commandService, commandServiceConfiguration) = ConfigureCommands();
            serviceCollection.AddSingleton(commandService);
            serviceCollection.AddSingleton(commandServiceConfiguration);

            // Other services
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
            serviceCollection.AddSingleton<ResponseCacheService>();
            serviceCollection.AddSingleton<AddonService>();
            serviceCollection.AddSingleton(new DataService(args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory));

            return serviceCollection.BuildServiceProvider();
        }

        private static (DiscordSocketClient discordClient, DiscordSocketConfig discordConfig) ConfigureDiscord()
        {
            var discordConfig = new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Warning,
                DefaultRetryMode = RetryMode.RetryTimeouts,
                AlwaysDownloadUsers = true
            };

            var discordClient = new DiscordSocketClient(discordConfig);

            return (discordClient, discordConfig);
        }

        private static (ICommandService commandService, CommandServiceConfiguration commandServiceConfiguration)
            ConfigureCommands()
        {
            var commandServiceConfig = new CommandServiceConfiguration
            {
                StringComparison = StringComparison.OrdinalIgnoreCase,
                DefaultRunMode = RunMode.Sequential,
                IgnoresExtraArguments = true,
                CooldownBucketKeyGenerator = (t, ctx, services) =>
                {
                    if (!(t is CooldownType ct))
                    {
                        throw new InvalidOperationException(
                           $"Cooldown bucket type is incorrect. Expected {typeof(CooldownType)}, received {t.GetType().Name}.");
                    }

                    var discordContext = ctx.Cast<AbyssRequestContext>();

                    if (discordContext.InvokerIsOwner)
                        return null; // Owners have no cooldown

                    return ct switch
                    {
                        CooldownType.Server => discordContext.Guild.Id.ToString(),

                        CooldownType.Channel => discordContext.Channel.Id.ToString(),

                        CooldownType.User => discordContext.Invoker.Id.ToString(),

                        CooldownType.Global => "Global",

                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            };

            var commandService = (ICommandService) new CommandService(commandServiceConfig);

            return (commandService, commandServiceConfig);
        }
    }
}