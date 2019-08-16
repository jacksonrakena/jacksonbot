using Discord;
using Discord.Commands;

namespace Abyss.Core.Entities
{
    public class AbyssRequestUpdateContext
    {
        public AbyssRequestUpdateContext(IUserMessage response, AbyssRequestContext request)
        {
            Response = response;
            Request = request;
        }

        public IUserMessage Response { get; }
        public AbyssRequestContext Request { get; }
    }
}