using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abyss.Results
{
    public class ResultCompletionData
    {
        public List<RestUserMessage> Messages { get; set; }

        public ResultCompletionData(params RestUserMessage[] messages)
        {
            Messages = messages.ToList();
        }
    }
}
