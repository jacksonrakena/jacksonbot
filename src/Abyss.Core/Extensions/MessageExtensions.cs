using Discord;
using System;

namespace Abyss.Core.Extensions
{
    public static class MessageExtensions
    {
        public static bool HasMentionPrefix(this IUserMessage msg, IUser user, ref int argPos)
        {
            var text = msg.Content;
            if (text.Length <= 3 || text[0] != '<' || text[1] != '@') return false;

            var endPos = text.IndexOf('>');
            if (endPos == -1) return false;
            if (text.Length < endPos + 2 || text[endPos + 1] != ' ') return false; //Must end in "> "

            if (!MentionUtils.TryParseUser(text.Substring(0, endPos + 1), out var userId)) return false;
            if (userId != user.Id) return false;
            argPos = endPos + 2;
            return true;
        }

        public static bool HasStringPrefix(this IUserMessage msg, string str, ref int argPos,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            var text = msg.Content;
            if (!text.StartsWith(str, comparisonType)) return false;
            argPos = str.Length;
            return true;
        }
    }
}