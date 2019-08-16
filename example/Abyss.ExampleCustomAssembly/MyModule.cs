using System.Threading.Tasks;
using Abyss.Core.Entities;
using Abyss.Core.Results;
using Discord;
using Discord.Commands;
using Qmmands;

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