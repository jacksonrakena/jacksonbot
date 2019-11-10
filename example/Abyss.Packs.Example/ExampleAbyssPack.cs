using System;
using System.Reflection;

namespace Abyss.Packs.Example
{
    public class ExampleAbyssPack : AbyssPack
    {
        public override string FriendlyName => "Example pack";
        public override string Description => "Example description";

        public override Assembly Assembly => typeof(ExampleAbyssPack).Assembly;
    }
}
