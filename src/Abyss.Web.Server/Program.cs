using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using Abyss.Core.Entities;
using Abyss.Shared.Hosts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

namespace Abyss.Web.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .Configure((buildingContext, applicationBuilder) =>
                {
                    if (buildingContext.HostingEnvironment.IsDevelopment())
                        applicationBuilder.UseDeveloperExceptionPage();
                    else
                    {
                        applicationBuilder.UseExceptionHandler("/Error");
                        applicationBuilder.UseHsts();
                    }

                    applicationBuilder.UseHttpsRedirection();
                    applicationBuilder.UseStaticFiles();
                    applicationBuilder.UseSpaStaticFiles();
                    applicationBuilder.UseRouting();

                    applicationBuilder.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller}/{action=Index}/{id?}");
                    });

                    applicationBuilder.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = "client";

                        if (buildingContext.HostingEnvironment.IsDevelopment())
                        {
                            spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                        }
                    });
                })
                .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
                {
                    configurationBuilder.AddCommandLine(args);
                    configurationBuilder.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    configurationBuilder.AddAbyssJsonFiles(hostingContext.HostingEnvironment.EnvironmentName);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddAbyssSentry();
                })
                .ConfigureKestrel(k =>
                {
                    k.ListenAnyIP(2003);
                })
                .ConfigureServices((hostBuildingContext, serviceCollection) =>
                {
                    serviceCollection.AddControllers();
                    serviceCollection.AddSpaStaticFiles(spa =>
                    {
                        spa.RootPath = "client/build";
                    });

                    // Abyss common service core
                    serviceCollection.ConfigureSharedServices();

                    // Configuration
                    var abyssConfig = new AbyssConfig();
                    hostBuildingContext.Configuration.Bind(abyssConfig);
                    serviceCollection.AddSingleton(abyssConfig);

                    // Application name
                    hostBuildingContext.HostingEnvironment.ApplicationName = abyssConfig.Name;
                })
                .Build();
        }
    }
}
