using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Abyss.Interactions;

public class InfinitePageView : PagedView
{
    public InfinitePageView(InfinitePageProvider pageProvider, LocalMessage templateMessage = null) : base(pageProvider, templateMessage)
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