using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Abyss.Addons
{
    public class AddonService
    {
        private readonly IServiceProvider _services;

        private readonly Type _addonType = typeof(IAddon);

        private readonly List<IAddon> _addons = new List<IAddon>();

        public AddonService(IServiceProvider services)
        {
            _services = services;
        }

        public Task AddAddonAsync<T>() where T : IAddon
        {
            return AddAddonAsync(typeof(T));
        }

        public async Task AddAddonAsync(Type type)
        {
            if (_addons.Any(a => a.GetType() == type)) throw new InvalidOperationException("An addon of type \"" + type.Name + "\" has already been registered.");

            if (!_addonType.IsAssignableFrom(type)) throw new InvalidOperationException("The provided type is not of the addon type, " + _addonType.Name + ".");

            if (type.IsInterface || type.IsAbstract) throw new InvalidOperationException("Cannot instantiate an abstract type, or an interface type.");

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0) throw new InvalidOperationException($"There are no public, instance-level constructors for {type.Name}.");
            if (constructors.Length > 1) throw new InvalidOperationException($"There are more than one public, instance-level constructors for {type.Name}.");
            var constructor = constructors[0];

            var parametersToExecute = new List<object>();
            var serviceProviderType = typeof(IServiceProvider);
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.ParameterType.IsAssignableFrom(serviceProviderType))
                {
                    parametersToExecute.Add(_services);
                    continue;
                }

                var service = _services.GetService(parameter.ParameterType);
                if (service == null)
                {
                    throw new InvalidOperationException($"Could not find a service for {parameter.ParameterType.Name}, while building an object of type {type.Name}.");
                }

                parametersToExecute.Add(service);
            }

            var addon = (IAddon) constructor.Invoke(parametersToExecute.ToArray());
            _addons.Add(addon);
            await addon.OnAddedAsync(_services);
        }

        public async Task RemoveAddonAsync(IAddon addon)
        {
            if (addon == null) return;
            await addon.OnRemovedAsync(_services);
            _addons.Remove(addon);
        }

        public Task RemoveAllAddonsAsync()
        {
            return Task.WhenAll(_addons.Select(RemoveAddonAsync));
        }
    }
}
