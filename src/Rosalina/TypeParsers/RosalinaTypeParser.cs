using Qmmands;

namespace Rosalina
{
    public abstract class RosalinaTypeParser<T> : TypeParser<T>
    {
#pragma warning disable IDE0051 // Remove unused private members
        (string Singular, string Multiple, string? Remainder) FriendlyName { get; }
#pragma warning restore IDE0051 // Remove unused private members
    }
}