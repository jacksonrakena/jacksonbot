using Abyssal.Common;
using Qmmands;
using System.Linq;

namespace Abyss
{
    public static class CommandsExtensions
    {
        public static bool IsGroup(this Module module)
        {
            return module.Type.HasCustomAttribute<GroupAttribute>();
        }

        public static string CreateCommandString(this Command command)
        {
            return $"{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => a.Name))}";
        }
    }
}