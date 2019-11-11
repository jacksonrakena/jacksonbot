using System;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Qmmands;
using Disqord;
using System.Collections.Generic;
using Disqord.Bot;
using System.Linq;
using AbyssalSpotify;
using Microsoft.Extensions.Configuration;

namespace Abyss
{
    /// <summary>
    ///     Extensions for dealing with service collections and providers.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configures the Abyss bot application.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add Abyss to.</param>
        /// <param name="acoAction">The configuration action for the host options.</param>
        /// <remarks>
        ///     An instance of <see cref="AbyssConfig"/> is expected to be added to <paramref name="serviceCollection"/> before this method is called.
        /// </remarks>
        public static IServiceCollection AddAbyssBot(this IServiceCollection serviceCollection, Action<IServiceProvider, AbyssHostOptions> acoAction)
        {
            serviceCollection.AddSingleton(provider =>
            {
                var abo = new AbyssHostOptions();
                acoAction(provider, abo);
                return abo;
            });

            serviceCollection.AddSingleton(provider =>
            {
                var ob = new AbyssConfig();
                provider.GetRequiredService<IConfiguration>().Bind(ob);
                return ob;
            });

            serviceCollection.AddSingleton(provider =>
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
                        CooldownBucketKeyGenerator = AbyssCooldownBucketKeyGenerators.Default,
                        DefaultArgumentParser = DefaultArgumentParser.Instance
                    }),
                    Prefixes = new List<string> { cfg.CommandPrefix },
                    // message cache default 100
                    ProviderFactory = bot => ((AbyssBot)bot).Services
                };
            });

            serviceCollection.AddSingleton<AbyssBot>();
            serviceCollection.AddSingleton<DataService>();
            serviceCollection.AddHostedService<AbyssHostedService>();
            serviceCollection.AddSingleton<HelpService>();
            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddSingleton<NotificationsService>();
            serviceCollection.AddSingleton<MarketingService>();
            serviceCollection.AddSingleton(provider =>
            {
                var configurationModel = provider.GetRequiredService<AbyssConfig>();
                return SpotifyClient.FromClientCredentials(configurationModel.Connections.Spotify.ClientId, configurationModel.Connections.Spotify.ClientSecret);
            });

            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddTransient<Random>();
            return serviceCollection;
        }
    }
}
