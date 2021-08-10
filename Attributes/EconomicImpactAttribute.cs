using System;

namespace Abyss.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EconomicImpactAttribute : Attribute
    {
        public EconomicImpactType ImpactType { get; }
        
        public EconomicImpactAttribute(EconomicImpactType type)
        {
            ImpactType = type;
        }
    }

    [Flags]
    public enum EconomicImpactType
    {
        /// <summary>
        ///     Users can gain coins.
        /// </summary>
        UserGainCoins,
        
        /// <summary>
        ///     Users can spend coins.
        /// </summary>
        UserSpendCoins,
        
        /// <summary>
        ///     Users send coins to each other, or trade items of value (neutral effect on economy)
        /// </summary>
        UserCoinNeutral
    }
}