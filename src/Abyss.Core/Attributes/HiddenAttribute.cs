using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HiddenAttribute : Attribute
    { }
}