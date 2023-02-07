namespace Jacksonbot.Persistence.Relational;

public abstract class RelationalRootObject
{
    public ValueTask OnCreatingAsync()
    {
        return default;
    }
}