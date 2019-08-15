using Microsoft.Extensions.Configuration;

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
