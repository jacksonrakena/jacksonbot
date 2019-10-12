using Abyss.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using Abyss.Hosting;
using Abyss.Core.Services;
using System.Reflection;

namespace Abyss.Console
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var contentRoot = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;
            System.Console.WriteLine($"Abyss console host application starting at {DateTime.Now:F}");

            var host = new HostBuilder()
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
                .ConfigureServices((hostBuildingContext, serviceCollection) =>
                {
                    // Configuration
                    var abyssConfig = new AbyssConfig();
                    hostBuildingContext.Configuration.Bind(abyssConfig);
                    serviceCollection.AddSingleton(abyssConfig);

                    // Application name
                    hostBuildingContext.HostingEnvironment.ApplicationName = "Abyss";

                    // Core services
                    serviceCollection.ConfigureSharedServices();
                })
                .UseConsoleLifetime()
                .Build();

            var receiver = host.Services.GetRequiredService<MessageReceiver>();
            receiver.LoadTypesFromAssembly(Assembly.LoadFrom("Abyss.Commands.Default.dll"));

            host.Run();
        }
    }
}