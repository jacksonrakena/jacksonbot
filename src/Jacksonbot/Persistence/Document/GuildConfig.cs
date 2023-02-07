using Jacksonbot.Persistence.Relational.Contexts;

namespace Jacksonbot.Persistence.Document;

public class GuildConfig : IAsyncCreatable
{
    public StarboardConfig Starboard { get; set; } = new();

    public Dictionary<GuildFeature, bool> Features { get; set; } = new();

    public ValueTask OnCreatingAsync(IServiceProvider provider)
    {
        return default;
    }
}