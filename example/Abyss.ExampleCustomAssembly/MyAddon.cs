using System;
using System.Threading.Tasks;
using Abyss.Core.Addons;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace Abyss.ExampleCustomAssembly
{
    public class MyAddon : IAddon
    {
        private readonly ILogger<MyAddon> _logger;

        public MyAddon(ILogger<MyAddon> logger)
        {
            _logger = logger;
        }

        public AddonDescriptor GetDescriptor()
        {
            return new AddonDescriptor
            {
                Author = "abyssal",
                Version = new Version(1, 0, 1),
                Description = "An example addon that shows how to use Abyss' addon platform.",
                FriendlyName = "Example Abyss Addon",
                Url = "https://github.com/abyssal512/Abyss/tree/master/example/Abyss.ExampleCustomAssembly"
            };
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
    }
}