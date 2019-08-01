using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abyss.Addons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Abyss.ExampleCustomAssembly
{
    public class MyAddon : IAddon
    {
        public string GetFriendlyName()
        {
            return "Abyss example addon.";
        }

        public Task OnAddedAsync(IServiceProvider services)
        {
            services.GetRequiredService<ILogger<MyAddon>>().LogInformation("Adding Abyss example addon...");
            return Task.CompletedTask;
        }

        public Task OnRemovedAsync(IServiceProvider services)
        {
            services.GetRequiredService<ILogger<MyAddon>>().LogInformation("Removing Abyss example addon...");
            throw new NotImplementedException();
        }
    }
}
