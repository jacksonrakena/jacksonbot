using System.Collections.Generic;

namespace Rosalina
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class RosalinaConfig
    {
        public string CommandPrefix { get; set; }

        public RosalinaConfigStartupSection Startup { get; set; }

        public RosalinaConfigConnectionsSection Connections { get; set; }

        public RosalinaConfigNotificationsSection Notifications { get; set; }

        public RosalinaConfigEmoteSection Emotes { get; set; }

        public RosalinaConfigMarketingSection Marketing { get; set; }
    }

    public class RosalinaConfigStartupSection
    {
        public IEnumerable<RosalinaConfigActivity> Activity { get; set; }
    }

    public class RosalinaConfigActivity
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class RosalinaConfigDiscordConnectionSection
    {
        public string Token { get; set; }
    }

    public class RosalinaConfigSpotifyConnectionSection
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class RosalinaConfigConnectionsSection
    {
        public RosalinaConfigDiscordConnectionSection Discord { get; set; }
        public RosalinaConfigSpotifyConnectionSection Spotify { get; set; }
    }

    public class RosalinaConfigNotificationsSection
    {
        public ulong? Ready { get; set; }
        public ulong? ServerMembershipChange { get; set; }
        public ulong? Feedback { get; set; }
        public ulong? Stopping { get; set; }
    }

    public class RosalinaConfigEmoteSection
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

    public class RosalinaConfigMarketingSection
    {
        public string DblDotComToken { get; set; }
        public string DiscordBoatsToken { get; set; }
        public string DiscordBotsListToken { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}