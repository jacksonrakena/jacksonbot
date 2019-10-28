using Disqord;

namespace Abyss
{
    public static class ConfigExtensions
    {
        public static string GetEmoteFromActivity(this AbyssConfigEmoteSection emoteSection, UserStatus status)
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
