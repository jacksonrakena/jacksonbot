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
                    applicationBuilder.UseResponseCompression();

                    if (buildingContext.HostingEnvironment.IsDevelopment())
                    {
                        applicationBuilder.UseDeveloperExceptionPage();
                        applicationBuilder.UseBlazorDebugging();
                    }

                    applicationBuilder.UseClientSideBlazorFiles<Client.Startup>();

                    applicationBuilder.UseRouting();

                    applicationBuilder.UseEndpoints(endpoints =>
                    {
                        endpoints.MapDefaultControllerRoute();
#if DEBUG
                        endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
#else
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("_content/abysswebclient/index.html");
                applicationBuilder.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(path1: Directory.GetCurrentDirectory(), "wwwroot/_content/abysswebclient")),
                    RequestPath = new PathString("")
                }); 
#endif
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
                })
                .ConfigureKestrel(k =>
                {
                    k.ListenAnyIP(2003);
                })
                .ConfigureServices((hostBuildingContext, serviceCollection) =>
                {
                    serviceCollection.AddMvc().AddNewtonsoftJson();
                    serviceCollection.AddResponseCompression(opts =>
                    {
                        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                            new[] { "application/octet-stream" });
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
