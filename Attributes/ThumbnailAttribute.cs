using System;

namespace Katbot.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ThumbnailAttribute : Attribute
    {
        public ThumbnailAttribute(string imageUrl)
        {
            ImageUrl = imageUrl;
        }

        public string ImageUrl { get; }
    }
}