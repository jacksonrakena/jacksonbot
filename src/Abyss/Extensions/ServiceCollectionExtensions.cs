using System;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Qmmands;
using Disqord;
using System.Collections.Generic;
using Disqord.Bot;
using System.Linq;

namespace Abyss
{
    /// <summary>
    ///     Extensions for dealing with service collections and providers.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configures the Abyss framework.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add Abyss to.</param>
        /// <param name="acoAction">The configuration action for the host options.</param>
        /// <typeparam name="TPackLoader">The pack loader to use for this framework instance.</typeparam>
        /// <remarks>
        ///     An instance of <see cref="AbyssConfig"/> is expected to be added to <paramref name="serviceCollection"/> before this method is called.
        /// </remarks>
        public static void AddAbyssFramework<TPackLoader>(this IServiceCollection serviceCollection, Action<IServiceProvider, AbyssHostOptions> acoAction) where TPackLoader : class, IPackLoader
        {
            serviceCollection.AddSingleton(provider =>
            {
                var abo = new AbyssHostOptions();
                acoAction(provider, abo);
                return abo;
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
            serviceCollection.AddSingleton<IPackLoader, TPackLoader>();
        }
    }
}
