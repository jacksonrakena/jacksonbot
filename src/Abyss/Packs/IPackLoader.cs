using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abyss
{
    /// <summary>
    ///     Represents a generic entity that can fetch packs for the Abyss framework to load.
    /// </summary>
    public interface IPackLoader
    {
        /// <summary>
        ///     Executes <see cref="AbyssBot.ImportPack{TPack}"/> on selected packs.
        /// </summary>
        /// <param name="bot">The bot to load packs into.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task LoadPacksAsync(AbyssBot bot);
    }
}
