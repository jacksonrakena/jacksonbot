using System;
using System.Collections;
using System.Linq;
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

        /// <summary>
        ///     Reads the value of a property.
        /// </summary>
        /// <param name="info">The property to read.</param>
        /// <param name="obj">The object to read the property from.</param>
        /// <returns>The value of the property.</returns>
        public static string ReadValue(this PropertyInfo info, object obj) => ReadValue((object) info, obj);

        /// <summary>
        ///     Reads the value of a field.
        /// </summary>
        /// <param name="info">The field to read.</param>
        /// <param name="obj">The object to read the field from.</param>
        /// <returns>The value of the field.</returns>
        public static string ReadValue(this FieldInfo info, object obj) => ReadValue((object) info, obj);

        private static string ReadValue(object prop, object obj)
        {

            /* PropertyInfo and FieldInfo both derive from MemberInfo, but that does not have a GetValue method, so the only
                supported ancestor is object */
            try
            {
                var value = prop switch
                {
                    PropertyInfo pinfo => pinfo.GetValue(obj),

                    FieldInfo finfo => finfo.GetValue(obj),

                    _ => throw new ArgumentException($"{nameof(prop)} must be PropertyInfo or FieldInfo", nameof(prop)),
                };

                if (value == null) return "Null";

                if (value is IEnumerable e && !(value is string))
                {
                    var enu = e.Cast<object>().ToList();
                    return $"{enu.Count} [{enu.GetType().Name}]";
                }
                else
                {
                    return value + $" [{value.GetType().Name}]";
                }
            }
            catch (Exception e)
            {
                return $"[[{e.GetType().Name} thrown]]";
            }
        }
    }
}
