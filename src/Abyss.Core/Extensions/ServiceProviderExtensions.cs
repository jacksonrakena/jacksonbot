using Abyss.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T InitializeService<T>(this IServiceProvider provider)
        {
            return provider.GetRequiredService<T>();
        }
    }
}