using Discord.Rest;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Core.Results
{
    public class ResultCompletionData
    {
        public List<RestUserMessage?> Messages { get; set; }

        public ResultCompletionData(params RestUserMessage?[] messages)
        {
            Messages = messages.ToList();
        }
    }
}