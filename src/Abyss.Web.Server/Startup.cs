using Abyss.Core.Addons;
using Abyss.Core.Services;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using AbyssalSpotify;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using System;
using System.Linq;
using System.Net.Http;
using Abyss.Core;

namespace Abyss.Web.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            var serviceCollection = services;

            // Bot core
            services.AddHostedService<BotService>();

            // Configuration
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(serviceProvider.GetRequiredService<DataService>().GetConfigurationBasePath())
                .AddJsonFile("Abyss.json", false, true);
                return configurationBuilder.Build();
            });
            serviceCollection.AddSingleton(serviceProvider =>
            {
                var configurationModel = new AbyssConfig();
                serviceProvider.GetRequiredService<IConfigurationRoot>().Bind(configurationModel);

                return configurationModel;
            });

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseClientSideBlazorFiles<Client.Startup>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
#if DEBUG
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
#else
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("_content/abysswebclient/index.html");
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(path1: Directory.GetCurrentDirectory(), "wwwroot/_content/abysswebclient")),
                    RequestPath = new PathString("")
                }); 
#endif
            });
        }

        private static (DiscordSocketClient discordClient, DiscordSocketConfig discordConfig) ConfigureDiscord()
        {
            var discordConfig = new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                LogLevel = LogSeverity.Warning,
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
