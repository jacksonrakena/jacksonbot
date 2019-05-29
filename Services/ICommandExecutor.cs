using System;
using System.Threading.Tasks;
using Katbot.Entities;
using Qmmands;

namespace Katbot.Services
{
    public interface ICommandExecutor
    {
        Task ExecuteAsync(KatbotCommandContext context, string content);

        Task HandleCommandExecutedAsync(CommandExecutedEventArgs args);

        int CommandFailures { get; }
        int CommandSuccesses { get; }
    }
}