using System;
using Abyssal.Common;
using AbyssalSpotify;
using Abyss.Persistence.Relational;
using Abyss.Services;
using Disqord.Bot.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Abyss
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                BuildAbyssHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Host terminated unexpectedly");
                return 1;
            }
        }

        public static IHost BuildAbyssHost(string[] runtimeArgs)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureDiscordBot((host, bot) =>
            {
                bot.Token = "Njc5OTI1OTY3MTUzOTIyMDU1.Xk4cZg.HIbQPmav8RgaNvfIqEkmTWezYhk";
                bot.Prefixes = new[] {"a."};
            });
            hostBuilder.UseSystemd();
            hostBuilder.ConfigureAppConfiguration(appConfigure =>
            {
                appConfigure.AddJsonFile(Constants.CONFIGURATION_FILENAME);
                appConfigure.AddEnvironmentVariables(Constants.ENVIRONMENT_VARIABLE_PREFIX);
            });
            hostBuilder.ConfigureLogging((hostContext, logging) =>
            {
                logging.AddConsole();    
            });
            var environment = Environment.GetEnvironmentVariable(Constants.ENVIRONMENT_VARNAME);
            if (environment == null)
            {
                Console.WriteLine(string.Format("{1} variable not set. Defaulting to {0}", Constants.DEFAULT_RUNTIME_ENVIRONMENT, Constants.ENVIRONMENT_VARNAME));
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
                .AddDbContext<AbyssPersistenceContext>(options =>
                {
                    options.UseNpgsql(builderContext.Configuration.GetConnectionString("Database"));
                })
                .AddSingleton(SpotifyClient.FromClientCredentials(spotify["ClientId"], spotify["ClientSecret"]))
                .AddSingleton<IActionScheduler, ActionScheduler>()
                .AddSingleton<HelpService>();
        }
    }
}
