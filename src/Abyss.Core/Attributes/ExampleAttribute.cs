using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute(params string[] exampleUsage)
        {
            ExampleUsage = exampleUsage;
        }

        public string[] ExampleUsage { get; }
    }
}