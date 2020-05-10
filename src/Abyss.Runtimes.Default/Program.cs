using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss.Runtimes.Default
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new HostBuilder()
                .UseEnvironment(
                    Environment.GetEnvironmentVariable("Abyss_Environment", EnvironmentVariableTarget.Process) ??
                    "Production")
                .ConfigureHostConfiguration(config =>
                {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    var botConfiguration = new DiscordBotConfiguration
                    {
                        Status = UserStatus.Online,
                        DefaultMentions = LocalMentions.NoEveryone,
                        ProviderFactory = bot => ((AbyssBot) bot).Services,
                        CommandServiceConfiguration = new CommandServiceConfiguration
                        {
                            CooldownBucketKeyGenerator = AbyssCooldownBucketKeyGenerators.Default
                        }
                    };
                    var prefixProvider = new DefaultPrefixProvider().AddMentionPrefix()
                        .AddPrefix(context.Configuration["Commands:Prefix"]);
                    var spotifyClient = SpotifyClient.FromClientCredentials(context.Configuration["Connections:Spotify:ClientId"],
                        context.Configuration["Connections:Spotify:ClientSecret"]);

                    services
                        .AddSingleton<IPrefixProvider>(prefixProvider)
                        .AddSingleton(botConfiguration)
                        .AddSingleton<AbyssBot>()
                        .AddSingleton<HttpClient>()
                        .AddSingleton<Random>()
                        .AddSingleton<HelpService>()
                        .AddSingleton(spotifyClient)
                        .AddHostedService<AbyssHostedService>();
                })
                .RunConsoleAsync().GetAwaiter().GetResult();
        }
    }
}
