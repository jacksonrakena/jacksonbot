using System;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<TValue>> Chunk<TValue>(this IEnumerable<TValue> values, Int32 chunkSize)
        {
            var valuesList = values.ToList();
            var count = valuesList.Count;        
            for (var i = 0; i < count / chunkSize + (count % chunkSize == 0 ? 0 : 1); i++)
            {
                yield return valuesList.Skip(i * chunkSize).Take(chunkSize);
            }
        }
    }
}