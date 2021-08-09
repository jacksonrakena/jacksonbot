using Abyss.Extensions;
using Disqord;
using Disqord.Bot;

namespace Abyss.Modules
{
    public abstract class AbyssGuildModuleBase : DiscordGuildModuleBase
    {
        protected virtual DiscordMenuCommandResult Pages(InfinitePageProvider pages)
        {
            return View(new InfinitePageView(pages));
        }

        public Color GetColor()
        {
            return Context.CurrentMember.GetHighestRoleColourOrDefault();
        }
    }
    
    public abstract class AbyssModuleBase : DiscordModuleBase
    {
        protected virtual DiscordMenuCommandResult Pages(InfinitePageProvider pages)
        {
            return View(new InfinitePageView(pages));
        }

        public Color GetColor()
        {
            return Context.Bot.CurrentUser.GetHighestRoleColourOrDefault();
        }
    }
}