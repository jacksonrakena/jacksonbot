using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Lament
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Lament runtime starting");
                BuildLamentHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHost BuildLamentHost(string[] runtimeArgs)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseSystemd();
            hostBuilder.UseSerilog();
            hostBuilder.ConfigureAppConfiguration(appConfigure =>
            {
                appConfigure.AddJsonFile(Constants.CONFIGURATION_FILENAME);
                appConfigure.AddEnvironmentVariables(Constants.ENVIRONMENT_VARIABLE_PREFIX);
            });
            var environment = Environment.GetEnvironmentVariable(Constants.ENVIRONMENT_VARNAME);
            if (environment == null)
            {
                Log.Error("{1} variable not set. Defaulting to {0}", Constants.DEFAULT_RUNTIME_ENVIRONMENT, Constants.ENVIRONMENT_VARNAME);
                environment = Constants.DEFAULT_RUNTIME_ENVIRONMENT;
            }
            hostBuilder.UseEnvironment(environment);
            hostBuilder.ConfigureServices(ConfigureServices);
            
            return hostBuilder.Build();
        }

        private static void ConfigureServices(HostBuilderContext builderContext, IServiceCollection serviceCollection)
        {
            
        }
    }
}