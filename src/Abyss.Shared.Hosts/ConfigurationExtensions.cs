using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Abyss.Shared.Hosts
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddAbyssJsonFiles(this IConfigurationBuilder configurationBuilder, string environmentName)
        {
            configurationBuilder.AddJsonFile("abyss.json", false, true);
            configurationBuilder.AddJsonFile($"abyss.{environmentName}.json", true, true);
            return configurationBuilder;
        }
    }
}
