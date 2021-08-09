using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abyss.Extensions;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Newtonsoft.Json.Linq;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Fun")]
    public class FunModule : AbyssModuleBase
    {
        [Command("cat")]
        [Description("Meow.")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.User)]
        public async Task<DiscordCommandResult> Command_GetCatPictureAsync()
        {
            return Pages(new CatPageProvider(this));
        }
    }

    public class CatPageProvider : InfinitePageProvider
    {
        private readonly AbyssModuleBase _caller;
        public CatPageProvider(AbyssModuleBase caller)
        {
            _caller = caller;
        }
        public override async ValueTask<Page> GetPageAsync(PagedViewBase view)
        {
            var url = _pages[view.CurrentPageIndex];
            if (url == null)
            {
                using var response = await new HttpClient().GetAsync("https://api.thecatapi.com/v1/images/search?size=full");
                url = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false))[0]
                    .Value<string>("url");
                _pages[view.CurrentPageIndex] = url;
            }
            return new Page().WithEmbeds(new LocalEmbed()
                .WithTitle("Enjoy your random cat.")
                .WithColor(_caller.GetColor())
                .WithImageUrl(url)
            ); 
        }

        public string[] _pages = new string[99];
        public override int PageCount { get; } = 99;
    }
}