using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Katbot.Common;
using Katbot.Entities;
using Katbot.Extensions;
using Katbot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Katbot
{
    public static class Program
    {
        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var services = ConfigureServices();

            var bot = services.GetRequiredService<BotService>();

            await bot.StartAsync().ConfigureAwait(false);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // Bot core
            serviceCollection.AddSingleton<BotService>();

            // Configuration
            var (configurationRoot, configurationModel) = ConfigureOptions();
            serviceCollection.AddSingleton(configurationModel);
            serviceCollection.AddSingleton(configurationRoot);

            // Logging
            serviceCollection.AddLogging(builder => builder.AddKatbot());

            // Discord
            var (discordClient, discordConfig) = ConfigureDiscord();
            serviceCollection.AddSingleton(discordClient);
            serviceCollection.AddSingleton(discordConfig);

            // Commands
            var (commandService, commandServiceConfiguration) = ConfigureCommands();
            serviceCollection.AddSingleton(commandService);
            serviceCollection.AddSingleton(commandServiceConfiguration);

            // Other services
            serviceCollection.AddSingleton<ApiStatisticsCollectionService>();
            serviceCollection.AddSingleton<HelpService>();
            serviceCollection.AddSingleton<IMessageProcessor, MessageProcessor>();
            serviceCollection.AddSingleton<ICommandExecutor, CommandExecutor>();
            serviceCollection.AddSingleton<ScriptingService>();
            serviceCollection.AddSingleton<SpoilerService>();
            serviceCollection.AddSingleton<SpotifyService>();
            serviceCollection.AddTransient<Random>();
            serviceCollection.AddSingleton<HttpClient>();

            return serviceCollection.BuildServiceProvider();
        }

        private static (IConfigurationRoot configurationRoot, KatbotConfig configurationModel) ConfigureOptions()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("katbot.json", false, true);
            var builtConfiguration = configurationBuilder.Build();

            var configurationModel = new KatbotConfig();
            builtConfiguration.Bind(configurationModel);

            return (builtConfiguration, configurationModel);
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

                    var discordContext = ctx.Cast<KatbotCommandContext>();

                    if (discordContext.InvokerIsOwner)
                        return null; // Owners have no cooldown

                    switch (ct)
                    {
                        case CooldownType.Server:
                            return discordContext.Guild.Id;

                        case CooldownType.Channel:
                            return discordContext.Channel.Id;

                        case CooldownType.User:
                            return discordContext.Invoker.Id;

                        case CooldownType.Global:
                            return "Global";

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            };

            var commandService = (ICommandService) new CommandService(commandServiceConfig);

            return (commandService, commandServiceConfig);
        }
    }
}