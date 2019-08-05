using Abyss.Results;
using Qmmands;
using System.Threading.Tasks;

namespace Abyss.ExampleCustomAssembly
{
    public class MyModule : Abyss.Entities.AbyssModuleBase
    {
        [Command("Test")]
        public Task<ActionResult> OkHandEmojiAsync()
        {
            return Ok(":ok_hand:");
        }
    }
}
