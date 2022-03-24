using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;

namespace Abyss.Interactions.GameMenu;

public class GameMenu : AbyssViewBase
{
    public GameMenu(DiscordCommandContext context) : base(new LocalMessage().WithEmbeds(
            new LocalEmbed()
                .WithTitle("Let's play a game!")
        )
    )
    {
        AddComponent(new SelectionViewComponent(async e =>
        {
                
        }));
    }
}