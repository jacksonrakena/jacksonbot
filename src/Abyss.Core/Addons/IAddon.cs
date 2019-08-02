using System;
using System.Threading.Tasks;

namespace Abyss.Addons
{
    /// <summary>
    ///     Represents an Abyss addon that can be added or removed at runtime.
    /// </summary>
    public interface IAddon
    {
        /// <summary>
        ///     This method is called when the addon is being added to the system.
        /// </summary>
        /// <param name="services">The service provider being used by the system.</param>
        /// <returns>An asynchronous task representing the adding operation.</returns>
        Task OnAddedAsync(IServiceProvider services);

        /// <summary>
        ///     This method is called when the addon is being removed from the system.
        /// </summary>
        /// <param name="services">The service provider being used by the system.</param>
        /// <returns>An asynchronous task representing the removing operation.</returns>
        Task OnRemovedAsync(IServiceProvider services);

        /// <summary>
        ///     This method returns information regarding the addon, like name, description, and author.
        /// </summary>
        AddonDescriptor GetDescriptor();
    }
}
