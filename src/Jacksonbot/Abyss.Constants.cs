using System.Reflection;
using Disqord;

namespace Jacksonbot;

public static class Constants
{
    public static Version VERSION => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
    public const string COPYRIGHT_ATTRIBUTION = "Jackson, 2017-2023";

    public const Markdown.TimestampFormat TIMESTAMP_FORMAT = Markdown.TimestampFormat.ShortDateTime;

    public static Color Theme => System.Drawing.Color.LightPink;

    public static readonly Guid SessionId = Guid.NewGuid();
}