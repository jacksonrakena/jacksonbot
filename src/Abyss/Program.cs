using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using Serilog;
using Serilog.Events;

namespace Abyss
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .UseContentRoot((args.Length > 0 && Directory.Exists(args[0])) ? args[0] : AppContext.BaseDirectory)
                .UseEnvironment(Environment.GetEnvironmentVariable("ABYSS_ENVIRONMENT", EnvironmentVariableTarget.Process) == "Development" ? "Development" : "Production")
                .UseDefaultServiceProvider(c =>
                {
                    c.ValidateOnBuild = true;
                    c.ValidateScopes = true;
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("abyss.json", false, true);
                    config.AddJsonFile($"abyss.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
                })
                .ConfigureLogging((hbc, logging) =>
                {
                    var logger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .WriteTo.Console(
                            outputTemplate: "[{Timestamp:HH:mm:ss yyyy-MM-dd} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                            restrictedToMinimumLevel: hbc.HostingEnvironment.IsDevelopment() ? LogEventLevel.Verbose : LogEventLevel.Information)
                        .WriteTo.File(
                            outputTemplate: "[{Timestamp:HH:mm:ss yyyy-MM-dd} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                            path: Path.Combine(hbc.HostingEnvironment.ContentRootPath, "logs", "abyss.log"),
                            restrictedToMinimumLevel: LogEventLevel.Information)
                        .CreateLogger();

                    logging.AddSerilog(logger, true);
                })
                .ConfigureServices(ConfigureServices);
        }

        public static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton(provider =>
                {
                    var ob = new AbyssConfig();
                    provider.GetRequiredService<IConfiguration>().Bind(ob);

                    if (ob.Startup.Activity == null || !ob.Startup.Activity.Any())
                        throw new Exception("Startup.Activity was not found in the configuration.");
                    return ob;
                })
                .AddSingleton(provider =>
                {
                    var cfg = provider.GetRequiredService<AbyssConfig>();
                    var act = cfg.Startup.Activity.First();
                    return new DiscordBotConfiguration
                    {
                        Activity = new LocalActivity(act.Message, act.Type),
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
                        Prefixes = new List<string> { cfg.CommandPrefix },
                        // message cache default 100
                        ProviderFactory = bot => ((AbyssBot)bot).Services
                    };
                })
                .AddSingleton<AbyssBot>()
                .AddHostedService<AbyssHostedService>()
                .AddSingleton<HelpService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<NotificationsService>()
                .AddSingleton<MarketingService>()
                .AddSingleton(provider =>
                {
                    var configurationModel = provider.GetRequiredService<AbyssConfig>();
                    return SpotifyClient.FromClientCredentials(configurationModel.Connections.Spotify.ClientId, configurationModel.Connections.Spotify.ClientSecret);
                })
                .AddSingleton<HttpClient>()
                .AddTransient<Random>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<ActionLogService>();
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
