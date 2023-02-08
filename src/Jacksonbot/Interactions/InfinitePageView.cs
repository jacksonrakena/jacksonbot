using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;

namespace Jacksonbot.Interactions;

/**
 * Slightly modified version of the original Disqord PagedView,
 * just making it work with infinite pages by removing the first/last buttons,
 * and the index in the footer.
 */

public class InfinitePageView : PagedViewBase
{
    public ButtonViewComponent NextPageButton { get; }

    public ButtonViewComponent StopButton { get; }

    public InfinitePageView(PageProvider pageProvider, Action<LocalMessageBase>? messageTemplate = null)
      : base(pageProvider, messageTemplate)
    {
        this.NextPageButton = new ButtonViewComponent(new ButtonViewComponentCallback(this.OnNextPageButton))
        {
            Emoji = new LocalEmoji("▶️"),
            Style = LocalButtonComponentStyle.Secondary
        };
        this.StopButton = new ButtonViewComponent(new ButtonViewComponentCallback(this.OnStopButton))
        {
            Emoji = new LocalEmoji("⏹️"),
            Style = LocalButtonComponentStyle.Secondary
        };
        this.AddComponent((ViewComponent)this.NextPageButton);
        this.AddComponent((ViewComponent)this.StopButton);
    }

    public override async ValueTask UpdateAsync()
    {
        CurrentPage = await PageProvider.GetPageAsync(this);
    }

    protected virtual ValueTask OnNextPageButton(ButtonEventArgs e)
    {
        ++this.CurrentPageIndex;
        return new ValueTask();
    }
    protected virtual ValueTask OnStopButton(ButtonEventArgs e)
    {
        if (this.Menu is DefaultMenuBase menu)
        {
            IUserMessage message = menu.Message;
            if (message != null)
                message.DeleteAsync();
        }
        this.Menu.Stop();
        return new ValueTask();
    }
}

public abstract class InfinitePageProvider : PageProvider
{
    public override int PageCount => int.MaxValue;
}