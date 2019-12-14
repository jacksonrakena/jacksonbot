using Disqord;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Rosalina
{
    /// <summary>
    ///     A check which requires that the current parameter must not be the invoker of the command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class MustNotBeInvokerAttribute : ParameterCheckAttribute, IRosalinaCheck
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            if (argument == null) return CheckResult.Successful;
            var id = argument is CachedUser user ? user.Id : argument is Snowflake dur ? dur : throw new InvalidOperationException($"{nameof(MustNotBeInvokerAttribute)} is being executed on an invalid object type.");

            return id == context.AsRosalinaContext().Invoker.Id
                ? new CheckResult("The provided user can't be you.")
                : CheckResult.Successful;
        }

        public string GetDescription(RosalinaCommandContext requestContext) => "The provided user can't be you.";

        /// <summary>
        ///     Initialises a new <see cref="MustNotBeInvokerAttribute"/>.
        /// </summary>
        public MustNotBeInvokerAttribute() : base(CheckResources.UserTypes)
        {
        }
    }
}