using Abyss.Core.Entities;
using Abyss.Core.Results;
using Qmmands;
using System.Threading.Tasks;

namespace Abyss.ExampleCustomAssembly
{
    public class MyModule : AbyssModuleBase
    {
        [Command("Test")]
        public Task<ActionResult> OkHandEmojiAsync()
        {
            return Ok(":ok_hand:");
        }
    }
}
