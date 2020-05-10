using Microsoft.Extensions.Logging;

namespace Abyss
{
    internal static class LoggingEventIds
    {
        // Success.
        public static readonly EventId CommandExecutedSuccess = new EventId(1, "Command executed");

        // Internal error.
        public static readonly EventId ExceptionThrownInPipeline = new EventId(100, "Exception thrown in parsing pipeline");
        public static readonly EventId UnknownResultType = new EventId(101, "Unknown result type");
        public static readonly EventId ExceptionThrownInCommand = new EventId(102, "Exception thrown in command");
        public static readonly EventId UnknownError = new EventId(103, "Unknown error");
        public static readonly EventId CommandReturnedBadType = new EventId(104, "Command returned bad type");
        public static readonly EventId CommandErroredRaisedWithUnknownType = new EventId(105, "CommandErrored event raised with unknown type");

        // User error.
        public static readonly EventId CommandExecutedUserFailure = new EventId(200, "Command executed - user failure");
        public static readonly EventId ChecksFailed = new EventId(201, "Checks failed");
        public static readonly EventId ParameterChecksFailed = new EventId(202, "Parameter checks failed");
        public static readonly EventId ArgumentParseFailed = new EventId(203, "Argument parse failed");
        public static readonly EventId TypeParserFailed = new EventId(204, "Type parser failed");
        public static readonly EventId CommandOnCooldown = new EventId(205, "Command on cooldown");
        public static readonly EventId OverloadNotFound = new EventId(206, "Overload not found");
        public static readonly EventId UserDirectMessaged = new EventId(207, "User sent a Direct Message - ignored");
        public static readonly EventId UnknownCommand = new EventId(208, "Unknown command");
    }
}