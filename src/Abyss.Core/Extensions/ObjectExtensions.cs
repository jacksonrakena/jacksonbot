namespace Abyss.Extensions
{
    public static class ObjectExtensions
    {
        public static T Cast<T>(this object @object)
        {
            return @object is T res ? res : default;
        }
    }
}