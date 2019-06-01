using Microsoft.Extensions.Logging;

namespace Abyss.Helpers
{
    public static class LoggingHelper
    {
        public static readonly EventId CommandExecutedUserFailure = new EventId(200, "Command executed - user failure");
        public static readonly EventId CommandExecutedSuccess = new EventId(1, "Command executed");
        public static readonly EventId ExceptionThrown = new EventId(100, "Exception thrown");
        public static readonly EventId ChecksFailed = new EventId(201, "Checks failed");
        public static readonly EventId ParameterChecksFailed = new EventId(202, "Parameter checks failed");
        public static readonly EventId ArgumentParseFailed = new EventId(203, "Argument parse failed");
        public static readonly EventId TypeParserFailed = new EventId(204, "Type parser failed");
        public static readonly EventId CommandOnCooldown = new EventId(205, "Command on cooldown");
        public static readonly EventId OverloadNotFound = new EventId(206, "Overload not found");
        public static readonly EventId UnknownResultType = new EventId(101, "Unknown result type");
    }
}