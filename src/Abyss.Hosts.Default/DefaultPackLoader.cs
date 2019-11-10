using Abyss.Packs.Default;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Hosts.Default
{
    public class DefaultPackLoader : IPackLoader
    {
        private readonly ILoggerFactory _factory;
        private readonly DataService _data;

        public DefaultPackLoader(ILoggerFactory factory, DataService data)
        {
            _factory = factory;
            _data = data;
        }

        public async Task LoadPacksAsync(AbyssBot bot)
        {
            var logger = _factory.CreateLogger("Abyss Host");
            var packBasePath = _data.GetPackBasePath();

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
                                await bot.ImportPackAsync(type);
                        }
                    }
                    catch (Exception)
                    {
                        logger.LogWarning($"Failed to load assembly from {file}.");
                    }
                }
            }
            else logger.LogWarning("Pack directory does not exist, skipping..");

            await bot.ImportPackAsync<DefaultAbyssPack>();
            _ = ScriptingHelper.EvaluateScriptAsync("2+2", new {}); // preload Roslyn
        }
    }
}
