using System.Threading.Tasks;

namespace Abyss.Core.Addons
{
    /// <summary>
    ///     Represents an Abyss addon that can be added or removed at runtime.
    /// </summary>
    public interface IAddon
    {
        /// <summary>
        ///     This method is called when the addon is being added to the system.
        /// </summary>
        /// <returns>An asynchronous task representing the adding operation.</returns>
        Task OnAddedAsync();

        /// <summary>
        ///     This method is called when the addon is being removed from the system.
        /// </summary>
        /// <returns>An asynchronous task representing the removing operation.</returns>
        Task OnRemovedAsync();

        /// <summary>
        ///     This method returns information regarding the addon, like name, description, and author.
        /// </summary>
        AddonDescriptor GetDescriptor();
    }
}
