using System;
using System.Threading.Tasks;
using Abyss.Core.Addons;
using Microsoft.Extensions.Logging;

namespace Abyss.ExampleCustomAssembly
{
    public class MyAddon : IAddon
    {
        private readonly ILogger<MyAddon> _logger;

        public AddonDescriptor GetDescriptor()
        {
            return new AddonDescriptor("abyssal", new Version(1, 0, 1), "Example Addon", "An example addon.", "https://github.com/abyssal512/Abyss/tree/master/example/Abyss.ExampleCustomAssembly");
        }

        public Task OnAddedAsync()
        {
            _logger.LogInformation("Adding Abyss example addon...");
            return Task.CompletedTask;
        }

        public Task OnRemovedAsync()
        {
            _logger.LogInformation("Removing Abyss example addon...");
            return Task.CompletedTask;
        }

        public MyAddon(ILogger<MyAddon> logger)
        {
            _logger = logger;
        }
    }
}
