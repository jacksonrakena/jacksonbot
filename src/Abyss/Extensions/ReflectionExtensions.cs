using System.Reflection;

namespace Abyss
{
    /// <summary>
    ///     Extensions related to reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        ///     Attempts to find the informational version of an assembly, otherwise defaults to the registered version.
        /// </summary>
        /// <param name="assembly">The assembly to query version for.</param>
        /// <returns>The best available format of the version of the provided assembly.</returns>
        public static string GetVersion(this Assembly assembly)
            => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version!.ToString();
    }
}
