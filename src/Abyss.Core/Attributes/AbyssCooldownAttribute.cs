using Qmmands;
using System;

namespace Abyss
{
    /// <summary>
    ///     The attribute for Abyss command cooldowns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AbyssCooldownAttribute : CooldownAttribute
    {
        /// <summary>
        ///     Initialises a new <see cref="AbyssCooldownAttribute"/>.
        /// </summary>
        /// <param name="amount">The number of requests to allow within the timeframe specified by <paramref name="per"/>.</param>
        /// <param name="per">The timeframe, or window, to allow <paramref name="amount"/> number of requests in.</param>
        /// <param name="cooldownMeasure">The unit in which <paramref name="per"/> is specified.</param>
        /// <param name="bucketType">The scope of this cooldown.</param>
        public AbyssCooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, CooldownType bucketType)
            : base(amount, per, cooldownMeasure, bucketType)
        {
        }
    }
}