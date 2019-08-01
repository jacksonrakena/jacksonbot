using Discord;

namespace Abyss.Entities
{
    public class AbyssRequestUpdateContext
    {
        public IUserMessage Response { get; }
        public AbyssRequestContext Request { get; }

        public AbyssRequestUpdateContext(IUserMessage response, AbyssRequestContext request)
        {
            Response = response;
            Request = request;
        }
    }
}