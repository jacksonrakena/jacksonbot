using System;
using Abyss.Entities;
using Qmmands;

namespace Abyss.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AbyssCooldownAttribute : CooldownAttribute
    {
        public AbyssCooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, CooldownType bucketType)
            : base(amount, per, cooldownMeasure, bucketType)
        {
        }
    }
}