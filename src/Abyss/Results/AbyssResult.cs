using Qmmands;
using System.Threading.Tasks;

namespace Abyss
{
    /// <summary>
    ///     Represents a generic Abyss result.
    /// </summary>
    public abstract class AbyssResult : CommandResult
    {
        /// <summary>
        ///     Executes this result with a context.
        /// </summary>
        /// <param name="context">The context of the invocation of which this result was returned.</param>
        /// <returns>A Task representing the asynchronous execution operation.</returns>
        public abstract Task<bool> ExecuteResultAsync(AbyssCommandContext context);

        public static implicit operator Task<AbyssResult>(AbyssResult res)
        {
            return Task.FromResult(res);
        }

        public static implicit operator ValueTask<AbyssResult>(AbyssResult res)
        {
            return new ValueTask<AbyssResult>(res);
        }

        public abstract object ToLog();
    }
}