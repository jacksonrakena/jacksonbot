using System;

namespace Abyss
{
    public static class Constants
    {
        public const string ENVIRONMENT_VARIABLE_PREFIX = "ABYSS_";
        public const string ENVIRONMENT_VARNAME = ENVIRONMENT_VARIABLE_PREFIX + "ENVIRONMENT";
        public const string DEFAULT_RUNTIME_ENVIRONMENT = "Development";

        public const string CONFIGURATION_FILENAME = "abyss.appsettings.json";

        public const string DEFAULT_GUILD_MESSAGE_PREFIX = "a.";

        public static Guid SessionId = Guid.NewGuid();
    }
}