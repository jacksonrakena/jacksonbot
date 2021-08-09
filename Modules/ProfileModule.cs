using System.Linq;
using System.Threading.Tasks;
using Abyss.Helpers;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Profile")]
    public class ProfileModule : AbyssGuildModuleBase
    {
        public AbyssPersistenceContext Database { get; set; }
        
        [Command("profile")]
        [Description("Look at your profile.")]
        public async Task<DiscordCommandResult> Profile(IMember member = null)
        {
            member ??= Context.Author;
            var profile = await Database.GetUserAccountsAsync(member.Id);

            var embed = new LocalEmbed()
                .WithColor(profile.Color)
                .WithAuthor(member)
                .WithDescription($"{member.Name}'s Abyss profile");

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
}