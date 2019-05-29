using System;

namespace Katbot.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DontAttachFooterAttribute : Attribute
    {
    }
}