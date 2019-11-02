using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Packs.Example
{
    [Name("Example")]
    public class ExampleModule : AbyssModuleBase
    {
        [Command("example")]
        public Task<ActionResult> ExampleCommand(string input)
        {
            return Ok($"You said: {input}");
        }
    }
}
