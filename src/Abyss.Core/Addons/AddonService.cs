using Abyssal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Core.Addons
{
    public class AddonService
    {
        private readonly IServiceProvider _services;

        private readonly List<IAddon> _addons = new List<IAddon>();

        public AddonService(IServiceProvider services)
        {
            _services = services;
        }

        public List<IAddon> GetAllAddons() => _addons;

        public Task AddAddonAsync<T>() where T : IAddon
        {
            return AddAddonAsync(typeof(T));
        }

        public async Task AddAddonAsync(Type type)
        {
            var addon = (IAddon) _services.Create(type);
            _addons.Add(addon);
            await addon.OnAddedAsync();
        }

        public async Task RemoveAddonAsync(IAddon addon)
        {
            if (addon == null) return;
            await addon.OnRemovedAsync();
            _addons.Remove(addon);
        }

        public Task RemoveAllAddonsAsync()
        {
            return Task.WhenAll(_addons.Select(RemoveAddonAsync));
        }
    }
}
