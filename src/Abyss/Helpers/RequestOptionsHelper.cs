using Discord;

namespace Abyss
{
    public static class RequestOptionsHelper
    {
        public static RequestOptions AuditLog(string auditLog)
        {
            return new RequestOptions { AuditLogReason = auditLog };
        }
    }
}