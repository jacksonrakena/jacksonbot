using System.Collections.Generic;

namespace Abyss
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class AbyssConfig
    {
        public string CommandPrefix { get; set; }

        public AbyssConfigStartupSection Startup { get; set; }

        public AbyssConfigConnectionsSection Connections { get; set; }

        public AbyssConfigNotificationsSection Notifications { get; set; }

        public AbyssConfigEmoteSection Emotes { get; set; }

        public AbyssConfigMarketingSection Marketing { get; set; }
    }

    public class AbyssConfigStartupSection
    {
        public class AbyssConfigActivity
        {
            public string Type { get; set; }
            public string Message { get; set; }
        }

        public IEnumerable<AbyssConfigActivity> Activity { get; set; }
    }

    public class AbyssConfigConnectionsSection
    {
        public class AbyssConfigDiscordConnectionSection
        {
            public string Token { get; set; }
        }

        public class AbyssConfigSpotifyConnectionSection
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        public AbyssConfigDiscordConnectionSection Discord { get; set; }
        public AbyssConfigSpotifyConnectionSection Spotify { get; set; }
    }

    public class AbyssConfigNotificationsSection
    {
        public ulong? Ready { get; set; }
        public ulong? ServerMembershipChange { get; set; }
        public ulong? Feedback { get; set; }
        public ulong? Stopping { get; set; }
    }

    public class AbyssConfigEmoteSection
    {
        public string YesEmote { get; set; }
        public string NoEmote { get; set; }
        public string AfkEmote { get; set; }
        public string OfflineEmote { get; set; }
        public string DndEmote { get; set; }
        public string StaffEmote { get; set; }
        public string GuildOwnerEmote { get; set; }
        public string OnlineEmote { get; set; }
    }

    public class AbyssConfigMarketingSection
    {
        public string DblDotComToken { get; set; }
        public string DiscordBoatsToken { get; set; }
        public string DiscordBotsListToken { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}