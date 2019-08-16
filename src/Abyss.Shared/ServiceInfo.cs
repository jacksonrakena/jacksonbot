using System;

namespace Abyss.Shared
{
    public class ServiceInfo
    {
        public string ServiceName { get; set; }
        public string Environment { get; set; }
        public string ProcessName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public int CommandSuccesses { get; set; }
        public int CommandFailures { get; set; }
        public int Guilds { get; set; }
        public int Users { get; set; }
        public int Channels { get; set; }
        public int Modules { get; set; }
        public int Commands { get; set; }
        public string OperatingSystem { get; set; }
        public string MachineName { get; set; }
        public int ProcessorCount { get; set; }
        public int CurrentThreadId { get; set; }
        public string RuntimeVersion { get; set; }
        public string Culture { get; set; }
        public string ContentRootPath { get; set; }
        public int AddonsLoaded { get; set; }
    }

    public class BotSupportServerInfo
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public int MemberCount { get; set; }
        public string GuildIconUrl { get; set; }
    }
}