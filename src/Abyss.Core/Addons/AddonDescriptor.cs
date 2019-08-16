using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Addons
{
    public class AddonDescriptor
    {
        /// <summary>
        ///     The author of this addon.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///     A URL that points to this addon's homepage or source code.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     A <see cref="Version" /> object representing the version of this addon.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        ///     The friendly name of this addon.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        ///     The description of this addon.
        /// </summary>
        public string Description { get; set; }
    }
}