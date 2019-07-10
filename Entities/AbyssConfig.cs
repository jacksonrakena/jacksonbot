using System.Collections.Generic;

namespace Abyss.Entities
{
    public class AbyssConfig
    {
        public string Name { get; set; }
        public string CommandPrefix { get; set; }

        public AbyssConfigStartupSection Startup { get; set; }

        public AbyssConfigConnectionsSection Connections { get; set; }

        public AbyssConfigNotificationsSection Notifications { get; set; }
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
    }
}