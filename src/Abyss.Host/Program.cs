using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Abyss.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var contentRoot = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;

            return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseContentRoot(contentRoot)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("abyss.json", false, true);
                    config.AddJsonFile($"abyss.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
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
