using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Abyssal.Common;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Qmmands.Delegates;
using Serilog;
using Serilog.Events;

namespace Adora
{
    public enum EnvironmentType
    {
        Staging,
        Development,
        Production
    }

    public class AdoraEnvironment
    {
        public EnvironmentType Environment { get; }
        public string ContentRoot { get; }

        public AdoraEnvironment(EnvironmentType environment, string contentRoot)
        {
            Environment = environment;
            ContentRoot = contentRoot;
        }
    }

    public static class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            var environment = Enum.Parse<EnvironmentType>(Environment.GetEnvironmentVariable("ROSALINA_ENVIRONMENT", EnvironmentVariableTarget.Process) ?? nameof(EnvironmentType.Development));
            var contentRoot = (args.Length > 0 && Directory.Exists(args[0])) ? args[0] : AppContext.BaseDirectory;
            var outputLoggingString = "[{Timestamp:HH:mm:ss yyyy-MM-dd} {Environment} {SourceContext} {Level:u3}] {Message:lj} {Properties}{NewLine}";
            var logFile = Path.Combine(contentRoot, "logs", "adora.log");
            if (File.Exists(logFile))
            {
                try
                {
                    File.Delete(logFile);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", environment.ToString())
                .WriteTo.Console(
                    outputTemplate: outputLoggingString,
                    restrictedToMinimumLevel: environment == EnvironmentType.Development ? LogEventLevel.Verbose : LogEventLevel.Information,
                    formatProvider: new CultureInfo("en-AU"))
                .WriteTo.File(
                    outputTemplate: outputLoggingString,
                    path: logFile,
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    flushToDiskInterval: new TimeSpan(0, 2, 0),
                    formatProvider: new CultureInfo("en-AU"))
                .CreateLogger();

            var hostLogger = Log.Logger.ForContext("SourceContext", "Adora Host");
            hostLogger.Information("Adora bot starting in mode {environment} at {time}.", environment, FormatHelper.FormatTime(DateTimeOffset.Now));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("adora.json", optional: false)
                .AddJsonFile($"adora.{environment}.json", optional: true)
                .Build();
            var configModel = new AdoraConfig();
            configuration.Bind(configModel);
            
            hostLogger.Information("Loaded configuration from {0} providers. {1} activities loaded.", configuration.Providers.Count(), configModel.Startup.Activity.Count());

            var startupActivity = configModel.Startup.Activity.FirstOrDefault() ?? throw new InvalidOperationException("No Startup.Activity supplied.");
            var botConfiguration = new DiscordBotConfiguration
            {
                Activity = new LocalActivity(startupActivity.Message, startupActivity.Type),
                Status = UserStatus.Online,
                CommandServiceConfiguration = new CommandServiceConfiguration
                {
                    StringComparison = StringComparison.OrdinalIgnoreCase,
                    DefaultRunMode = RunMode.Sequential,
                    IgnoresExtraArguments = true,
                    CooldownBucketKeyGenerator = CooldownKeyGenerator,
                    DefaultArgumentParser = DefaultArgumentParser.Instance
                },
                // Message cache default: 100
                ProviderFactory = bot => ((AdoraBot)bot).Services
            };
            var prefixProvider = new DefaultPrefixProvider();
            prefixProvider.AddMentionPrefix();
            prefixProvider.AddPrefix(configModel.CommandPrefix);
            var spotifyClient = SpotifyClient.FromClientCredentials(configModel.Connections.Spotify.ClientId, configModel.Connections.Spotify.ClientSecret);

            var serviceCollection = new ServiceCollection()
                // Environment
                .AddSingleton(new AdoraEnvironment(environment, contentRoot))
                // Configuration
                .AddSingleton(configuration)
                // Configuration (strong type)
                .AddSingleton(configModel)
                // Bot configuration
                .AddSingleton<IPrefixProvider>(prefixProvider)
                .AddSingleton(botConfiguration)
                // Bot
                .AddSingleton<AdoraBot>()
                // Core services
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
            var bot = services.GetRequiredService<AdoraBot>();
            var hostLogger = Log.Logger.ForContext("SourceContext", "Adora Host");

            var addTypeParser = typeof(AdoraBot).GetMethod("AddTypeParser");
            if (addTypeParser == null) throw new InvalidOperationException("AddTypeParser method missing.");
            foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (!(type.GetCustomAttribute<DiscoverableTypeParserAttribute>() is { } dtpa)) continue;
                var method = addTypeParser.MakeGenericMethod(type.BaseType!.GetGenericArguments()[0]);
                method.Invoke(bot, new[] { services.Create(type), dtpa.ReplacingPrimitive });
                hostLogger.Information("Added parser {parser}.", type.Name);
            }
            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                if (!_cts.IsCancellationRequested) _cts.Cancel();
            };

            // CTRL+C handler 
            Console.CancelKeyPress += (sender, args) =>
            {
                args.Cancel = true;
                if (!_cts.IsCancellationRequested) _cts.Cancel();
            };

            var run = true;
            while (run)
            {
                _cts = new CancellationTokenSource();
                try
                {
                    hostLogger.Information("Adora Discord service starting at {time}.", FormatHelper.FormatTime(DateTimeOffset.Now));

                    await bot.RunAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    hostLogger.Information("Discord service has stopped.");
                    Log.CloseAndFlush();
                    run = false;
                    _cts.Dispose();
                    continue;
                }
                catch (Exception e)
                {
                    hostLogger.Error(e, "Root-level exception occurred that killed the bot.");
                    _cts.Dispose();
                    continue;
                }
                hostLogger.Error("The Discord service task stopped blocking. Restarting in 5 minutes...");
                await Task.Delay(TimeSpan.FromMinutes(5));
            }    
        }

        public static CooldownBucketKeyGeneratorDelegate CooldownKeyGenerator = (t, ctx) =>
        {
            if (!(t is CooldownType ct))
            {
                throw new InvalidOperationException(
                   $"Cooldown bucket type is incorrect. Expected {typeof(CooldownType)}, received {t.GetType().Name}.");
            }

            var discordContext = ctx.AsAdoraContext();

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
