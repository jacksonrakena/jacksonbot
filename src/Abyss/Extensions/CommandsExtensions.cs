using Abyssal.Common;
using Qmmands;
using System.Linq;

namespace Abyss
{
    /// <summary>
    ///     Extensions to the command processing library.
    /// </summary>
    public static class CommandsExtensions
    {
        /// <summary>
        ///     Determines whether the specified module is a group module.
        /// </summary>
        /// <param name="module">The module to check.</param>
        /// <returns>A boolean indicating whether the specified module is a group module.</returns>
        public static bool IsGroup(this Module module)
        {
            return module.Type.HasCustomAttribute<GroupAttribute>();
        }

        /// <summary>
        ///     Creates a command string from the full aliases and parameters of a command.
        /// </summary>
        /// <param name="command">The command to format.</param>
        /// <returns>The full alias of a command, followed by the names of the command parameters.</returns>
        public static string CreateCommandString(this Command command)
        {
            return $"{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => "{" + a.Name + "}"))}";
        }
    }
}