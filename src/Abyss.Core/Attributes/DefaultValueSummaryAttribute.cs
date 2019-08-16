using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DefaultValueDescriptionAttribute : Attribute
    {
        public DefaultValueDescriptionAttribute(string defaultValueDescription)
        {
            DefaultValueDescription = defaultValueDescription;
        }

        public string DefaultValueDescription { get; }
    }
}