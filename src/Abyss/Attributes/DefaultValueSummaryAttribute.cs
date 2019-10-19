using System;

namespace Abyss
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DefaultValueDescriptionAttribute : Attribute
    {
        public DefaultValueDescriptionAttribute(string defaultValueDescription)
        {
            DefaultValueDescription = defaultValueDescription;
        }

        public string DefaultValueDescription { get; }
    }
}