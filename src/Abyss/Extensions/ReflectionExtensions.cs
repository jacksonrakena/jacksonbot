using System.Reflection;

namespace Abyss
{
    /// <summary>
    ///     Extensions related to reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        ///     Retrieves best-format version of an assembly.
        /// </summary>
        /// <param name="assembly">Assembly to query version for.</param>
        /// <returns>Best available format of an assembly version.</returns>
        public static string GetVersion(this Assembly assembly)
            => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version!.ToString();
    }
}
