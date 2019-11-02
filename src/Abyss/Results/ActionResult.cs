using Qmmands;
using System.Threading.Tasks;

namespace Abyss
{
    /// <summary>
    ///     Represents a generic Abyss result.
    /// </summary>
    public abstract class ActionResult : CommandResult
    {
        /// <summary>
        ///     Executes this result with a context.
        /// </summary>
        /// <param name="context">The context of the invocation of which this result was returned.</param>
        /// <returns>A Task representing the asynchronous execution operation.</returns>
        public abstract Task ExecuteResultAsync(AbyssRequestContext context);

        public static implicit operator Task<ActionResult>(ActionResult res)
        {
            return Task.FromResult(res);
        }

        public static implicit operator ValueTask<ActionResult>(ActionResult res)
        {
            return new ValueTask<ActionResult>(res);
        }
    }
}