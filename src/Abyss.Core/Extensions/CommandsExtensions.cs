using Qmmands;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Abyss.Core.Extensions
{
    public static class CommandsExtensions
    {
        public static bool IsGroup(this Module module)
        {
            return module.Type.GetCustomAttributes(typeof(GroupAttribute), true).Length != 0;
        }

        public static string CreateCommandString(this Command command)
        {
            return $"{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => a.Name))}";
        }
    }
}