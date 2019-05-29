namespace Katbot.Parsers
{
    public interface IKatbotTypeParser
    {
        (string Singular, string Multiple, string Remainder) FriendlyName { get; }
    }
}