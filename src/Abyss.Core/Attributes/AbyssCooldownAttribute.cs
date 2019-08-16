using System;
using Abyss.Core.Entities;
using Discord;
using Discord.Commands;
using Qmmands;

namespace Abyss.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AbyssCooldownAttribute : CooldownAttribute
    {
        public AbyssCooldownAttribute(int amount, double per, CooldownMeasure cooldownMeasure, CooldownType bucketType)
            : base(amount, per, cooldownMeasure, bucketType)
        { }
    }
}