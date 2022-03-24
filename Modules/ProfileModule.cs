using System.Linq;
using System.Threading.Tasks;
using Abyss.Helpers;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Abyss.Modules;

[Name("Profile")]
[Group("profile")]
public class ProfileModule : AbyssModuleBase
{
    public AbyssDatabaseContext Database { get; set; }

    [Command("color", "colour", "c")]
    [Description("Change your profile colour.")]
    public async Task<DiscordCommandResult> ColorSet(Color color)
    {
        var profile = await Database.GetUserAccountAsync(Context.Author.Id);
        profile.ColorR = color.R;
        profile.ColorG = color.G;
        profile.ColorB = color.B;
        await Database.SaveChangesAsync();
        return Reply("Changed your color to " + color.ToString() + ".");
    }
        
    [Command("bio", "desc", "description")]
    [Description("Change your profile bio..")]
    public async Task<DiscordCommandResult> Description(string bio)
    {
        var profile = await Database.GetUserAccountAsync(Context.Author.Id);
        profile.Description = bio;
        await Database.SaveChangesAsync();
        return Reply("Changed your description.");
    }
        
    [Command]
    [Description("Look at your profile.")]
    public async Task<DiscordCommandResult> Profile(IMember member = null)
    {
        member ??= Context.Author;
        var profile = await Database.GetUserAccountAsync(member.Id);

        var embed = new LocalEmbed()
            .WithColor(profile.Color)
            .WithAuthor(member)
            .WithThumbnailUrl(member.GetAvatarUrl(size: 1024));
        embed.WithDescription(string.IsNullOrWhiteSpace(profile.Description) 
            ? Context.Author.Id == member.Id ? "You haven't set your bio yet. Try `a.profile bio <bio>`." : "This user hasn't sent their bio yet." 
            : profile.Description);

        embed.AddField(":coin: Coins", profile.Coins, true);
        if (profile.FirstInteraction != null)
            embed.AddField("First interaction", Markdown.Timestamp(profile.FirstInteraction.Value, Constants.TIMESTAMP_FORMAT));
            
        if (profile.LatestInteraction != null)
            embed.AddField("Latest interaction", Markdown.Timestamp(profile.LatestInteraction.Value, Constants.TIMESTAMP_FORMAT));

        if (profile.BadgesString.Length > 0)
        {
            embed.AddField("Badges", string.Join(", ", profile.Badges));
        }

        return Reply(embed);
    }
}