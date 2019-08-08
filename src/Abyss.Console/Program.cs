using Abyss.Core.Addons;
using Abyss.Core.Services;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using AbyssalSpotify;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Qmmands;
using System;
using System.Net.Http;
using Abyss.Core;

namespace Abyss.Console
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            System.Console.WriteLine("Abyss console host application starting at " + DateTime.Now.ToString("F"));

            var contentRoot = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;

            var host = new HostBuilder()
                .UseContentRoot(contentRoot)
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.SetBasePath(contentRoot);
                    hostConfig.AddJsonFile("hostsettings.json", optional: true);
                    hostConfig.AddEnvironmentVariables("Abyss_");
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("abyss.json", false, true);
                    config.AddJsonFile($"abyss.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .RunConsoleAsync();

            host.GetAwaiter().GetResult();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<BotService>();

            // Configuration
            var abyssConfig = new AbyssConfig();
            context.Configuration.Bind(abyssConfig);
            serviceCollection.AddSingleton(abyssConfig);

            context.HostingEnvironment.ApplicationName = abyssConfig.Name;

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
            serviceCollection.AddSingleton<NotificationsService>();
            serviceCollection.AddSingleton<DataService>();

        }

        private static (DiscordSocketClient discordClient, DiscordSocketConfig discordConfig) ConfigureDiscord()
        {
            var discordConfig = new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Debug,
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