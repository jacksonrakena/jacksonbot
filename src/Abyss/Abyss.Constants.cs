using System;
using Disqord;

namespace Abyss;

public static class Constants
{
    public static Version VERSION = new Version(19, 3, 0);
    public const string ENVIRONMENT_VARIABLE_PREFIX = "ABYSS_";
    public const string ENVIRONMENT_VARNAME = ENVIRONMENT_VARIABLE_PREFIX + "ENVIRONMENT";
    public const string DEFAULT_RUNTIME_ENVIRONMENT = "Development";

    public const string CONFIGURATION_FILENAME = "abyss.appsettings.json";


    public const Markdown.TimestampFormat TIMESTAMP_FORMAT = Markdown.TimestampFormat.ShortDateTime;

    public static Color Theme => System.Drawing.Color.LightPink;

    public static Guid SessionId = Guid.NewGuid();
}