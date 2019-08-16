using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Rest;

namespace Abyss.Core.Results
{
    public class ResultCompletionData
    {
        public ResultCompletionData(params RestUserMessage[] messages)
        {
            Messages = messages.ToList();
        }

        public List<RestUserMessage> Messages { get; set; }
    }
}