using System;

namespace Abyss.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DontAttachTimestampAttribute : Attribute
    {
    }
}