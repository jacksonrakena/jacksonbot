using System.Collections.Generic;

namespace Katbot.Entities
{
    public class KatbotConfig
    {
        public string Name { get; set; }
        public string CommandPrefix { get; set; }

        public KatbotConfigStartupSection Startup { get; set; }

        public KatbotConfigConnectionsSection Connections { get; set; }
    }

    public class KatbotConfigStartupSection
    {
        public class KatbotConfigActivity
        {
            public string Type { get; set; }
            public string Message { get; set; }
        }

        public IEnumerable<KatbotConfigActivity> Activity { get; set; }
    }

    public class KatbotConfigConnectionsSection
    {
        public class KatbotConfigDiscordConnectionSection
        {
            public string Token { get; set; }
        }

        public class KatbotConfigSpotifyConnectionSection
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        public KatbotConfigDiscordConnectionSection Discord { get; set; }
        public KatbotConfigSpotifyConnectionSection Spotify { get; set; }
    }
}