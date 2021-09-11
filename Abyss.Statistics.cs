using Prometheus;

namespace Abyss
{
    public class AbyssStatistics
    {
        public static readonly Counter TotalCommandsExecuted = Metrics.CreateCounter(
            name: "abyss_commands_executed",
            help: "The total amount of commands executed.",
            "result");

        public static readonly Gauge CachedGuilds = Metrics.CreateGauge(
            name: "abyss_cached_guilds",
            help: "The number of guilds held in cache.");

        public static readonly Gauge MemoryUsage = Metrics.CreateGauge(
            name: "abyss_process_memory_usage",
            help: "The total amount of memory being used by the Abyss process.");
    }
}