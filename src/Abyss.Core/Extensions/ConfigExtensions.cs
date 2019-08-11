using System;
using System.Collections.Generic;
using System.Text;
using Abyss.Core.Entities;
using Discord;

namespace Abyss.Core.Extensions
{
    public static class ConfigExtensions
    {
        public static string GetEmoteFromActivity(this AbyssConfigEmoteSection emoteSection, UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Offline:
                    return emoteSection.OfflineEmote;
                case UserStatus.Online:
                    return emoteSection.OnlineEmote;
                case UserStatus.Idle:
                    return emoteSection.AfkEmote;
                case UserStatus.AFK:
                    return emoteSection.AfkEmote;
                case UserStatus.DoNotDisturb:
                    return emoteSection.DndEmote;
                case UserStatus.Invisible:
                    return emoteSection.OfflineEmote;
                default:
                    return emoteSection.OfflineEmote;
            }
        }
    }
}
