using System;

namespace Adora
{
    /// <summary>
    ///     Provides a description of the default value of a parameter, inside a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DefaultValueDescriptionAttribute : Attribute
    {
        /// <summary>
        ///     Initialises a new <see cref="DefaultValueDescriptionAttribute"/>.
        /// </summary>
        /// <param name="defaultValueDescription">The description of the default value of this parameter.</param>
        public DefaultValueDescriptionAttribute(string defaultValueDescription)
        {
            DefaultValueDescription = defaultValueDescription;
        }

        /// <summary>
        ///     The description of the default value of this parameter.
        /// </summary>
        public string DefaultValueDescription { get; }
    }
}