using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Serilog;
using Serilog.Events;

namespace Abyss
{
    public enum EnvironmentType
    {
        Staging,
        Development,
        Production
    }

    public class AbyssEnvironment
    {
        public EnvironmentType Environment { get; }
        public string ContentRoot { get; }

        public AbyssEnvironment(EnvironmentType environment, string contentRoot)
        {
            Environment = environment;
            ContentRoot = contentRoot;
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            var environment = Enum.Parse<EnvironmentType>(Environment.GetEnvironmentVariable("ABYSS_ENVIRONMENT", EnvironmentVariableTarget.Process) ?? nameof(EnvironmentType.Development));
            var contentRoot = (args.Length > 0 && Directory.Exists(args[0])) ? args[0] : AppContext.BaseDirectory;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss yyyy-MM-dd} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}{Context}",
                    restrictedToMinimumLevel: environment == EnvironmentType.Development ? LogEventLevel.Verbose : LogEventLevel.Information,
                    formatProvider: new CultureInfo("en-AU"))
                .WriteTo.File(
                    outputTemplate: "[{Timestamp:HH:mm:ss yyyy-MM-dd} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}{Context}",
                    path: Path.Combine(contentRoot, "logs", "abyss.log"),
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    flushToDiskInterval: new TimeSpan(0, 2, 0),
                    formatProvider: new CultureInfo("en-AU"))
                .CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Log.CloseAndFlush();

            var hostLogger = Log.Logger.ForContext("SourceContext", "Abyss Host");
            hostLogger.Information("Abyss bot starting in mode {environment} at {time}.", environment, FormatHelper.FormatTime(DateTimeOffset.Now));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("abyss.json", optional: false)
                .AddJsonFile($"abyss.{environment}.json", optional: true)
                .Build();
            var configModel = new AbyssConfig();
            configuration.Bind(configModel);

            var startupActivity = configModel.Startup.Activity.FirstOrDefault() ?? throw new InvalidOperationException("No Startup.Activity supplied.");
            var botConfiguration = new DiscordBotConfiguration
            {
                Activity = new LocalActivity(startupActivity.Message, startupActivity.Type),
                Status = UserStatus.Online,
                HasMentionPrefix = true,
                CommandService = new CommandService(new CommandServiceConfiguration
                {
                    StringComparison = StringComparison.OrdinalIgnoreCase,
                    DefaultRunMode = RunMode.Sequential,
                    IgnoresExtraArguments = true,
                    CooldownBucketKeyGenerator = CooldownKeyGenerator,
                    DefaultArgumentParser = DefaultArgumentParser.Instance
                }),
                Prefixes = new List<string> { configModel.CommandPrefix },
                // Message cache default: 100
                ProviderFactory = bot => ((AbyssBot)bot).Services
            };
            var spotifyClient = SpotifyClient.FromClientCredentials(configModel.Connections.Spotify.ClientId, configModel.Connections.Spotify.ClientSecret);

            var serviceCollection = new ServiceCollection()
                // Environment
                .AddSingleton(new AbyssEnvironment(environment, contentRoot))
                // Configuration
                .AddSingleton(configuration)
                // Configuration (strong type)
                .AddSingleton(configModel)
                // Bot configuration
                .AddSingleton(botConfiguration)
                // Bot
                .AddSingleton<AbyssBot>()
                // Core services
                .AddSingleton<MarketingService>()
                .AddSingleton<NotificationsService>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<ActionLogService>()
                .AddSingleton<HelpService>()
                // Command services
                .AddSingleton<HttpClient>()
                .AddSingleton<Random>()
                // Spotify
                .AddSingleton(spotifyClient);

            var services = serviceCollection.BuildServiceProvider();

            StartAsync(services).GetAwaiter().GetResult();
        }

        public static async Task StartAsync(IServiceProvider services)
        {
            var bot = services.GetRequiredService<AbyssBot>();
            var hostLogger = Log.Logger.ForContext("SourceContext", "Abyss Host");

            try
            {
                hostLogger.Information("Abyss bot host starting at {time}.", FormatHelper.FormatTime(DateTimeOffset.Now));
                bot.GetRequiredService<NotificationsService>();
                Console.CancelKeyPress += async (sender, args) =>
                {
                    await services.GetRequiredService<NotificationsService>().NotifyStoppingAsync().ConfigureAwait(false);
                    hostLogger.Information("Received cancel key press. Stopping...");
                    await bot.StopAsync().ConfigureAwait(false);
                    hostLogger.Information("Stopped Discord service.");
                };
                await bot.RunAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                hostLogger.Error(e, "Exception occurred that killed the bot.");
            }
        }

        public static CooldownBucketKeyGeneratorDelegate CooldownKeyGenerator = (t, ctx) =>
        {
            if (!(t is CooldownType ct))
            {
                throw new InvalidOperationException(
                   $"Cooldown bucket type is incorrect. Expected {typeof(CooldownType)}, received {t.GetType().Name}.");
            }

            var discordContext = ctx.AsAbyssContext();

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
        };
    }
}
