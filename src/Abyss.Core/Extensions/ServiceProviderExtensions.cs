using Microsoft.Extensions.DependencyInjection;
using System;

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