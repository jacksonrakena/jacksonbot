using System;

namespace Katbot.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class HiddenAttribute : Attribute
    {
    }
}