using Abyss.Persistence.Document;
using Abyss.Persistence.Relational;
using Abyss.Modules.Abstract;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Qmmands;

namespace Abyss.Modules;

public class GuildModule : AbyssGuildModuleBase
{
    [SlashCommand("feature")]
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