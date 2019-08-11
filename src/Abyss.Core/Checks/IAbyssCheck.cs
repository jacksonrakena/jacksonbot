using Abyss.Core.Entities;

namespace Abyss.Core.Checks
{
    public interface IAbyssCheck
    {
        string GetDescription(AbyssRequestContext requestContext);
    }
}