using Discord;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Abyss.Core.Entities
{
    public class UserEqualityComparer : IEqualityComparer<IUser>
    {
        public bool Equals([AllowNull] IUser x, [AllowNull] IUser y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] IUser obj)
        {
            return obj.GetHashCode();
        }
    }
}
