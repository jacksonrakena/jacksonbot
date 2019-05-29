using System;
using Katbot.Entities;
using Qmmands;

namespace Katbot.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class KatbotCooldownAttribute : CooldownAttribute
    {
        public KatbotCooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, CooldownType bucketType)
            : base(amount, per, cooldownMeasure, bucketType)
        {
        }
    }
}