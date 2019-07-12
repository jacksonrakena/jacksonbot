namespace Abyss.Parsers
{
    public interface IAbyssTypeParser
    {
        (string Singular, string Multiple, string Remainder) FriendlyName { get; }
    }
}