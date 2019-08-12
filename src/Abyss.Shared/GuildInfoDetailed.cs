using System;

namespace Abyss.Shared
{
    public class GuildInfoDetailed
    {
        public string AfkChannel { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? AbyssJoinDate { get; set; }
        public string Roles { get; set; }
        public string TextChannels { get; set; }
        public string VoiceChannels { get; set; }
    }
}
