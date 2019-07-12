using Discord;

namespace Abyss.Entities
{
    public class AbyssUpdateContext
    {
        public IUserMessage Response { get; }
        public AbyssCommandContext Request { get; }

        public AbyssUpdateContext(IUserMessage response, AbyssCommandContext request)
        {
            Response = response;
            Request = request;
        }
    }
}