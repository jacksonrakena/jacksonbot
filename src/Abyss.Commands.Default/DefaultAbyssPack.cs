using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    public class DefaultAbyssPack : AbyssPack
    {
        public override string FriendlyName => "Abyss Commands - Default";

        public override Assembly Assembly => typeof(DefaultAbyssPack).Assembly;
    }
}
