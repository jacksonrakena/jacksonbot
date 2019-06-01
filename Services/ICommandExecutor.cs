using Abyss.Entities;
using Qmmands;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public interface ICommandExecutor
    {
        Task ExecuteAsync(AbyssCommandContext context, string content);

        Task HandleCommandExecutedAsync(CommandExecutedEventArgs args);

        int CommandFailures { get; }
        int CommandSuccesses { get; }
    }
}