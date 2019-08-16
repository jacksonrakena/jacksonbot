using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SilentCheckAttribute : Attribute
    { }
}