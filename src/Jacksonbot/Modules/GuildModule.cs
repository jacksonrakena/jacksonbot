using Disqord.Bot.Commands.Interaction;
using Jacksonbot.Modules.Abstract;
using Jacksonbot.Persistence.Document;
using Jacksonbot.Persistence.Relational.Contexts;
using Qmmands;

namespace Jacksonbot.Modules;

public class GuildModule : BotGuildModuleBase
{
    //[SlashCommand("feature")]
    [Disqord.Bot.Commands.RequireGuildOwner]
    [Qmmands.Description("Enables or disables a guild feature.")]
    public async Task<DiscordInteractionResponseCommandResult> ToggleFeatureAsync([Name("Feature")] GuildFeature feature)
    {
        var gc = await Database.GetGuildConfigAsync(Context.GuildId);

        var value = gc.Features.ContainsKey(feature) && !gc.Features[feature];

        await Database.GuildConfigurations.ModifyObjectAsync(Context.GuildId, f =>
        {
            f.Features[feature] = value;
        });

        return Response($"{(value ? "Enabled" : "Disabled")} feature {feature}.");
    }
}