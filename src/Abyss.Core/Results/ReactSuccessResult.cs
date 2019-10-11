using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abyss.Core.Entities;
using Discord;

namespace Abyss.Core.Results
{
    public class ReactSuccessResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssRequestContext context)
        {
            return context.Message.AddReactionAsync(new Emoji("👌"));
        }
    }
}
