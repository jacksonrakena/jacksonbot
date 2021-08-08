using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Abyss.Extensions
{
    public class InfinitePageView : PagedView
    {
        public InfinitePageView(PageProvider pageProvider, LocalMessage templateMessage = null) : base(pageProvider, templateMessage)
        {
        }

        protected override void ApplyPageIndex(Page page)
        {
            // no-op
        }
    }

    public abstract class InfinitePageProvider : PageProvider
    {
        public override int PageCount => int.MaxValue;
    }

    public abstract class AbyssModuleBase : DiscordModuleBase
    {
        protected virtual DiscordMenuCommandResult Pages(InfinitePageProvider pages)
        {
            return View(new InfinitePageView(pages));
        }
    }
}