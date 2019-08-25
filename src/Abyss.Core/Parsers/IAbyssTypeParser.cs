namespace Abyss.Core.Parsers
{
    public interface IAbyssTypeParser
    {
        (string Singular, string Multiple, string? Remainder) FriendlyName { get; }
    }
}