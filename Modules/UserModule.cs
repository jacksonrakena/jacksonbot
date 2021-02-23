using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Lament.Discord;
using Qmmands;

namespace Lament.Modules
{
    public class UserModule : LamentModuleBase
    {
        [Command("avatar", "av", "a", "pic", "pfp")]
        [Description("Grabs the avatar for a user.")]
        public async Task GetAvatarAsync(
            [Name("User")]
            [Description("The user who you wish to get the avatar for.")]
            CachedMember target = null)
        {
            target ??= Context.Member;
            await ReplyAsync(embed: new LocalEmbedBuilder()
                .WithAuthor(target)
                .WithColor(Context.Color)
                .WithImageUrl(target.GetAvatarUrl())
                .WithDescription($"{UrlHelper.CreateMarkdownUrl("128", target.GetAvatarUrl(size: 128))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("256", target.GetAvatarUrl(size: 256))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("1024", target.GetAvatarUrl(size: 1024))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("2048", target.GetAvatarUrl(size: 2048))}")
                .Build());
        }
    }
}