using Discord;

namespace Abyss.Core.Helpers
{
    public static class RequestOptionsHelper
    {
        public static RequestOptions AuditLog(string auditLog)
        {
            return new RequestOptions { AuditLogReason = auditLog };
        }
    }
}