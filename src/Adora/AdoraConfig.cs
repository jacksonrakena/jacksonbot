using System.Collections.Generic;

namespace Adora
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class AdoraConfig
    {
        public string CommandPrefix { get; set; }

        public AdoraConfigStartupSection Startup { get; set; }

        public AdoraConfigConnectionsSection Connections { get; set; }

        public AdoraConfigNotificationsSection Notifications { get; set; }

        public AdoraConfigEmoteSection Emotes { get; set; }

        public AdoraConfigMarketingSection Marketing { get; set; }
    }

    public class AdoraConfigStartupSection
    {
        public IEnumerable<AdoraConfigActivity> Activity { get; set; }
    }

    public class AdoraConfigActivity
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class AdoraConfigDiscordConnectionSection
    {
        public string Token { get; set; }
    }

    public class AdoraConfigSpotifyConnectionSection
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class AdoraConfigConnectionsSection
    {
        public AdoraConfigDiscordConnectionSection Discord { get; set; }
        public AdoraConfigSpotifyConnectionSection Spotify { get; set; }
    }

    public class AdoraConfigNotificationsSection
    {
        public ulong? Ready { get; set; }
        public ulong? ServerMembershipChange { get; set; }
        public ulong? Feedback { get; set; }
        public ulong? Stopping { get; set; }
    }

    public class AdoraConfigEmoteSection
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

    public class AdoraConfigMarketingSection
    {
        public string DblDotComToken { get; set; }
        public string DiscordBoatsToken { get; set; }
        public string DiscordBotsListToken { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}