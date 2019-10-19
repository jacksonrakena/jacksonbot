using System;

namespace Abyss
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class ThumbnailAttribute : Attribute
    {
        public ThumbnailAttribute(string imageUrl)
        {
            ImageUrl = imageUrl;
        }

        public string ImageUrl { get; }
    }
}