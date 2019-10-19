using Qmmands;
using System;

namespace Abyss
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AbyssCooldownAttribute : CooldownAttribute
    {
        public AbyssCooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, CooldownType bucketType)
            : base(amount, per, cooldownMeasure, bucketType)
        {
        }
    }
}