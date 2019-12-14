using Disqord;

namespace Rosalina
{
    /// <summary>
    ///     Extensions related to <see cref="RosalinaConfig"/>.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        ///     Converts a <see cref="UserStatus"/> to the appropriate emoji.
        /// </summary>
        /// <param name="emoteSection">The config emote section which contains the status emojis.</param>
        /// <param name="status">The status to convert.</param>
        /// <returns>A string which represents the emoji for the status, or the offline emoji if the user status is invalid.</returns>
        public static string GetStatusEmote(this RosalinaConfigEmoteSection emoteSection, UserStatus status)
        {
            return status switch
            {
                UserStatus.Offline => emoteSection.OfflineEmote,
                UserStatus.Online => emoteSection.OnlineEmote,
                UserStatus.Idle => emoteSection.AfkEmote,
                UserStatus.DoNotDisturb => emoteSection.DndEmote,
                UserStatus.Invisible => emoteSection.OfflineEmote,
                _ => emoteSection.OfflineEmote,
            };
        }
    }
}
