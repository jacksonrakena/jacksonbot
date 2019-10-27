using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
                .ConfigureServices(serviceColl =>
                {
                    serviceColl.ConfigureAbyss(abyss =>
                    {
                        abyss.DataRoot = dataRoot;
                    });
                })
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
                    webBuilder.SuppressStatusMessages(false);
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseSetting(WebHostDefaults.ApplicationKey, "Abyss");
                    webBuilder.ConfigureKestrel(kestrel =>
                    {
                        kestrel.ListenAnyIP(2110);
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
