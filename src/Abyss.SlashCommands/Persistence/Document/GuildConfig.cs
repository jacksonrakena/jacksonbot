using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Persistence.Document;

public class GuildConfig : IAsyncCreatable
{
    public List<string> Prefixes { get; set; }

    public StarboardConfig Starboard { get; set; } = new();

    public GuildFeatureMatrix Features { get; set; } = new();
        
    public ValueTask OnCreatingAsync(IServiceProvider provider)
    {
        Prefixes = new List<string> {provider.GetRequiredService<IConfiguration>().GetSection("Options")["DefaultPrefix"]};
        return default;
    }
}