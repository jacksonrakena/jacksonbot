using System;

namespace Katbot.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DontAttachTimestampAttribute : Attribute
    {
    }
}