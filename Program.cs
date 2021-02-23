using System;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Lament.Discord;
using Lament.Logging;
using Lament.Persistence;
using Lament.Persistence.Relational;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;
using Serilog;
using Serilog.Events;

namespace Lament
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                BuildLamentHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHost BuildLamentHost(string[] runtimeArgs)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseSystemd();
            hostBuilder.ConfigureAppConfiguration(appConfigure =>
            {
                appConfigure.AddJsonFile(Constants.CONFIGURATION_FILENAME);
                appConfigure.AddEnvironmentVariables(Constants.ENVIRONMENT_VARIABLE_PREFIX);
            });
            hostBuilder.ConfigureLogging((hostContext, logging) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostContext.Configuration)
                    .CreateLogger();
                logging.ClearProviders();
                logging.AddSerilog();
            });
            var environment = Environment.GetEnvironmentVariable(Constants.ENVIRONMENT_VARNAME);
            if (environment == null)
            {
                Log.Error("{1} variable not set. Defaulting to {0}", Constants.DEFAULT_RUNTIME_ENVIRONMENT, Constants.ENVIRONMENT_VARNAME);
                environment = Constants.DEFAULT_RUNTIME_ENVIRONMENT;
            }
            hostBuilder.UseEnvironment(environment);
            hostBuilder.ConfigureServices(ConfigureServices);

            return hostBuilder.Build();
        }

        public static void ConfigureServices(HostBuilderContext builderContext, IServiceCollection serviceCollection)
        {
            var secrets = builderContext.Configuration.GetSection("Secrets");
            var spotify = secrets.GetSection("Spotify");
            serviceCollection
                .AddMemoryCache()
                .AddDbContext<LamentPersistenceContext>(options =>
                {
                    options.UseNpgsql(builderContext.Configuration.GetConnectionString("Database"));
                })
                .AddSingleton<DisqordLogger>()
                .AddSingleton(SpotifyClient.FromClientCredentials(spotify["ClientId"], spotify["ClientSecret"]))
                .AddSingleton<IPrefixProvider, LamentPrefixProvider>()
                .AddSingleton(services => new DiscordBotConfiguration
                {
                    ProviderFactory = (_) => services,
                    CommandServiceConfiguration = new CommandServiceConfiguration
                    {
                        IgnoresExtraArguments = true,
                        SeparatorRequirement = SeparatorRequirement.SeparatorOrWhitespace,
                        CooldownBucketKeyGenerator = CooldownBucketKeyGenerators.Default
                    },
                    Logger = services.GetRequiredService<DisqordLogger>()
                })
                .AddHostedService<LamentServiceHost>()
                .AddSingleton<LamentDiscordBot>()
                .Add(ServiceDescriptor.Singleton(typeof(DiscordBot), typeof(LamentDiscordBot)));
        }
    }
}