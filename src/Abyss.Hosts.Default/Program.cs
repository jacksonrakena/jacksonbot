using System;
using System.IO;
using System.Reflection;
using Abyss.Commands.Default;
using AbyssalSpotify;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Abyss.Hosts.Default
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var dataRoot = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(dataRoot)) dataRoot = AppDomain.CurrentDomain.BaseDirectory; // IIS tomfoolery

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(dataRoot);
                    config.AddJsonFile("abyss.json", false, true);
                    config.AddJsonFile($"abyss.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
                })
                .ConfigureServices(serviceColl => ConfigureServices(dataRoot, serviceColl))
                .UseDefaultServiceProvider(c =>
                {
                    c.ValidateOnBuild = true;
                    c.ValidateScopes = true;
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(Configure);
                    webBuilder.SuppressStatusMessages(false);
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseSetting(WebHostDefaults.ApplicationKey, "Abyss");
                });
        }

        public static void ConfigureServices(string dataRoot, IServiceCollection services)
        {
            // ASP.NET
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // Configuration
            services.AddSingleton(p =>
            {
                var ob = new AbyssConfig();
                p.GetRequiredService<IConfiguration>().Bind(ob);
                return ob;
            });

            // Abyss framework
            services.AddAbyssFramework((provider, botOptions) =>
            {
                botOptions.DataRoot = dataRoot;
            });

            // Abyss.Commands.Default
            services.AddSingleton(provider =>
            {
                var configurationModel = provider.GetRequiredService<AbyssConfig>();
                return SpotifyClient.FromClientCredentials(configurationModel.Connections.Spotify.ClientId, configurationModel.Connections.Spotify.ClientSecret);
            });
            services.AddTransient<Random>();
        }

        public static void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("Abyss Host");
            var dataService = app.ApplicationServices.GetRequiredService<DataService>();
            var packBasePath = dataService.GetCustomAssemblyBasePath();
            var bot = app.ApplicationServices.GetRequiredService<AbyssBot>();

            if (Directory.Exists(packBasePath))
            {
                foreach (var file in Directory.GetFiles(packBasePath, "*.dll"))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        foreach (var type in assembly.GetExportedTypes())
                        {
                            if (typeof(AbyssPack).IsAssignableFrom(type))
                                bot.ImportPack(type);
                        }
                    }
                    catch (Exception)
                    {
                        logger.LogWarning($"Failed to load assembly from {file}.");
                    }
                }
            }
            else logger.LogWarning("Pack directory does not exist, skipping..");

            bot.ImportPack<DefaultAbyssPack>();
        }
    }
}
