using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Jacksonbot.Modules.Abstract;
using Qmmands;

namespace Jacksonbot.Modules;

[Name("Profile")]
[SlashGroup("profile")]
public class ProfileModule : BotModuleBase
{
    [SlashCommand("color")]
    [Description("Change your profile colour.")]
    public async Task<DiscordInteractionResponseCommandResult> ColorSet(
        [Description("A color hex value, starting with a #.")] Color color)
    {
        var profile = await Database.GetUserAccountAsync(Context.Author.Id);
        profile.ColorR = color.R;
        profile.ColorG = color.G;
        profile.ColorB = color.B;
        await Database.SaveChangesAsync();
        return Response("Changed your color to " + color.ToString() + ".");
    }

    [SlashCommand("bio")]
    [Description("Change your profile bio.")]
    public async Task<DiscordInteractionResponseCommandResult> Description(string bio)
    {
        var profile = await Database.GetUserAccountAsync(Context.Author.Id);
        profile.Description = bio;
        await Database.SaveChangesAsync();
        return Response("Changed your description.");
    }

    [SlashCommand("view")]
    [Description("Look at your profile.")]
    public async Task<DiscordInteractionResponseCommandResult> Profile(IUser? member = null)
    {
        member ??= Context.Author;
        var profile = await Database.GetUserAccountAsync(member.Id);

        var embed = new LocalEmbed()
            .WithColor(profile.Color ?? Color.Pink)
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

        return Response(embed);
    }
}

public class User_ProfileModule : BotModuleBase
{
    [UserCommand("Look at their profile")]
    public async Task<DiscordInteractionResponseCommandResult> Profile(IUser member)
    {
        var profile = await Database.GetUserAccountAsync(member.Id);

        var embed = new LocalEmbed()
            .WithColor(profile.Color ?? Color.Pink)
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

        return Response(embed);
    }
}