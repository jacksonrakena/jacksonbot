using System;

namespace Abyss.Discord
{
    [Flags]
    public enum RuntimeFlags
    {
        None,
        DryRun,
        Verbose,
        RunAsOther
    }
}