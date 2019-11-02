using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Abyss
{
    public class AbyssCorePack : AbyssPack
    {
        public override string FriendlyName => $"Abyss Core";

        public override Assembly Assembly => typeof(AbyssCorePack).Assembly;

        public AbyssCorePack(AbyssBot bot)
        {
            bot.AddArgumentParser(UnixArgumentParser.Instance);
        }
    }
}
