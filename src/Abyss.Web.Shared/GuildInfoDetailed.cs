using System;
using System.Collections.Generic;
using System.Text;

namespace Abyss.Web.Shared
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
