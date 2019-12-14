using Qmmands;
using System;

namespace Rosalina
{
    /// <summary>
    ///     The attribute for Rosalina command cooldowns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RosalinaCooldownAttribute : CooldownAttribute
    {
        /// <summary>
        ///     Initialises a new <see cref="RosalinaCooldownAttribute"/>.
        /// </summary>
        /// <param name="amount">The number of requests to allow within the timeframe specified by <paramref name="per"/>.</param>
        /// <param name="per">The timeframe, or window, to allow <paramref name="amount"/> number of requests in.</param>
        /// <param name="cooldownMeasure">The unit in which <paramref name="per"/> is specified.</param>
        /// <param name="bucketType">The scope of this cooldown.</param>
        public RosalinaCooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, CooldownType bucketType)
            : base(amount, per, cooldownMeasure, bucketType)
        {
        }
    }
}