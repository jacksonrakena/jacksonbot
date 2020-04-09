using Qmmands;

namespace Adora
{
    public abstract class AdoraTypeParser<T> : TypeParser<T>
    {
#pragma warning disable IDE0051 // Remove unused private members
        (string Singular, string Multiple, string? Remainder) FriendlyName { get; }
#pragma warning restore IDE0051 // Remove unused private members
    }
}