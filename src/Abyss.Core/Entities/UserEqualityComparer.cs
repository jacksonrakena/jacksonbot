using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
