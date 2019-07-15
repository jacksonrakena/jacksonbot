using System;
using System.Collections.Generic;
using System.Text;

namespace Abyss.Web.Shared
{
    public class BotStatusInfo
    {
        public string Username { get; set; }
        public string Id { get; set; }
        public int Guilds { get; set; }
        public BotSupportServerInfo SupportServerInfo { get; set; }
    }

    public class BotSupportServerInfo
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public int MemberCount { get; set; }
        public string GuildIconUrl { get; set; }
    }
}
