using System.ComponentModel;

namespace Abyss.Persistence.Document;

/// <summary>
///     Describes the enabled binary features in a guild.
/// </summary>
public class GuildFeatureMatrix : RoleChannelIgnorable
{
    [DisplayName("touchgrass")]
    public bool TouchGrassEnabled { get; set; } = false;
}