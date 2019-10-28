using Disqord;

namespace Abyss
{
    public static class RestRequestOptionsHelper
    {
        public static RestRequestOptions AuditLog(string auditLog)
        {
            return RestRequestOptions.FromReason(auditLog);
        }
    }
}