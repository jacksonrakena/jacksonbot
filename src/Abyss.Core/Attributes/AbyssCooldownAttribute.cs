using Abyss.Core.Entities;
using Qmmands;
using System;

namespace Abyss.Core.Attributes
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