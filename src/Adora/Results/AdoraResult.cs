using Qmmands;
using System.Threading.Tasks;

namespace Adora
{
    /// <summary>
    ///     Represents a generic Adora result.
    /// </summary>
    public abstract class AdoraResult : CommandResult
    {
        /// <summary>
        ///     Executes this result with a context.
        /// </summary>
        /// <param name="context">The context of the invocation of which this result was returned.</param>
        /// <returns>A Task representing the asynchronous execution operation.</returns>
        public abstract Task<bool> ExecuteResultAsync(AdoraCommandContext context);

        public static implicit operator Task<AdoraResult>(AdoraResult res)
        {
            return Task.FromResult(res);
        }

        public static implicit operator ValueTask<AdoraResult>(AdoraResult res)
        {
            return new ValueTask<AdoraResult>(res);
        }

        public abstract object ToLog();
    }
}