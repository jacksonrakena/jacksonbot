using System.Collections.Generic;

namespace Adora
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class AdoraConfig
    {
        public string CommandPrefix { get; set; }

        public AdoraConfigStartupSection Startup { get; set; }

        public AdoraConfigConnectionsSection Connections { get; set; }
        
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
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}