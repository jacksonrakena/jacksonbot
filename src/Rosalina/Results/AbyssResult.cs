using Qmmands;
using System.Threading.Tasks;

namespace Rosalina
{
    /// <summary>
    ///     Represents a generic Rosalina result.
    /// </summary>
    public abstract class RosalinaResult : CommandResult
    {
        /// <summary>
        ///     Executes this result with a context.
        /// </summary>
        /// <param name="context">The context of the invocation of which this result was returned.</param>
        /// <returns>A Task representing the asynchronous execution operation.</returns>
        public abstract Task<bool> ExecuteResultAsync(RosalinaCommandContext context);

        public static implicit operator Task<RosalinaResult>(RosalinaResult res)
        {
            return Task.FromResult(res);
        }

        public static implicit operator ValueTask<RosalinaResult>(RosalinaResult res)
        {
            return new ValueTask<RosalinaResult>(res);
        }

        public abstract object ToLog();
    }
}