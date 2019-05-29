using Discord;

namespace Katbot.Entities
{
    public class KatbotUpdateContext
    {
        public IUserMessage Response { get; }
        public KatbotCommandContext Request { get; }

        public KatbotUpdateContext(IUserMessage response, KatbotCommandContext request)
        {
            Response = response;
            Request = request;
        }
    }
}