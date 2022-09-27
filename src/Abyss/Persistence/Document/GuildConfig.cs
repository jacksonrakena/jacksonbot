using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Persistence.Document;

public class GuildConfig : IAsyncCreatable
{
    public StarboardConfig Starboard { get; set; } = new();

    public Dictionary<GuildFeature, bool> Features { get; set; } = new();

    public ValueTask OnCreatingAsync(IServiceProvider provider)
    {
        return default;
    }
}