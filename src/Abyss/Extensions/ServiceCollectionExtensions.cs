using System;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Qmmands;
using Microsoft.Extensions.Hosting;
using Disqord;
using Disqord.Bot.Parsers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Disqord.Bot;
using System.Linq;

namespace Abyss
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAbyssFramework(this IServiceCollection serviceCollection, Action<IServiceProvider, AbyssHostOptions> acoAction)
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
                    MessageCacheSize = 100,
                    ProviderFactory = bot => 
                    {
                        Console.WriteLine("ProviderFactory being accessed");
                        return ((AbyssBot)bot).Services;
                    }
                };
            });

            serviceCollection.AddSingleton<AbyssBot>();
            serviceCollection.AddSingleton<DataService>();
            serviceCollection.AddHostedService<AbyssHostedService>();
            serviceCollection.AddSingleton<HelpService>();
            serviceCollection.AddSingleton<HttpClient>();
            serviceCollection.AddSingleton<NotificationsService>();
            serviceCollection.AddSingleton<MarketingService>();
        }
    }
}
