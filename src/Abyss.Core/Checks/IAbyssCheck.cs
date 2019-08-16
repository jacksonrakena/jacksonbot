using Abyss.Core.Entities;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Checks
{
    public interface IAbyssCheck
    {
        string GetDescription(AbyssRequestContext requestContext);
    }
}