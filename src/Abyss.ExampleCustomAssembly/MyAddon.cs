using System;
using System.Threading.Tasks;
using Abyss.Addons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Abyss.ExampleCustomAssembly
{
    public class MyAddon : IAddon
    {
        public AddonDescriptor GetDescriptor()
        {
            return new AddonDescriptor
            {
                Author = "abyssal",
                Version = new Version(1, 0, 1),
                Description = "An example addon that shows how to use Abyss' addon platform.",
                FriendlyName = "Example Abyss Addon",
                Url = "https://github.com/abyssal512/Abyss/tree/master/src/Abyss.ExampleCustomAssembly"
            };
        }

        public Task OnAddedAsync(IServiceProvider services)
        {
            services.GetRequiredService<ILogger<MyAddon>>().LogInformation("Adding Abyss example addon...");
            return Task.CompletedTask;
        }

        public Task OnRemovedAsync(IServiceProvider services)
        {
            services.GetRequiredService<ILogger<MyAddon>>().LogInformation("Removing Abyss example addon...");
            return Task.CompletedTask;
        }
    }
}
