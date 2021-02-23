using System;

namespace Lament.Discord
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