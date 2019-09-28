using System;
using System.Diagnostics;
using System.Globalization;

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
        public string? AvatarUrl { get; set; }
        public string? UsernameDiscriminator { get; set; }

        public ServiceInfo(string serviceName, string environmentName, Process currentProcess, int commandSuccesses,
            int commandFailures, int guildCount, int guildMemberCount, int channels, int moduleCount, int commandCount, string contentRootPath,
                int addonCount, string? avatarUrl2048, string? usernameDiscriminator)
        {
            ServiceName = serviceName;
            Environment = environmentName;
            ProcessName = currentProcess.ProcessName;
            StartTime = currentProcess.StartTime;
            CommandSuccesses = commandSuccesses;
            CommandFailures = commandFailures;
            Guilds = guildCount;
            Users = guildMemberCount;
            Channels = channels;
            Modules = moduleCount;
            Commands = commandCount;
            OperatingSystem = System.Environment.OSVersion.VersionString;
            MachineName = System.Environment.MachineName;
            ProcessorCount = System.Environment.ProcessorCount;
            CurrentThreadId = System.Environment.CurrentManagedThreadId;
            RuntimeVersion = System.Environment.Version.ToString();
            Culture = CultureInfo.InstalledUICulture.EnglishName;
            ContentRootPath = contentRootPath;
            AddonsLoaded = addonCount;
            AvatarUrl = avatarUrl2048;
            UsernameDiscriminator = usernameDiscriminator;
        }
    }
}
