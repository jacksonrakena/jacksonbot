using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Packs.Default
{
    public class DefaultAbyssPack : AbyssPack
    {
        public override string FriendlyName => "Default Abyss Pack";

        public override Assembly Assembly => typeof(DefaultAbyssPack).Assembly;
    }
}
