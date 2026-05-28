using System;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class HealthUp : IParameterUpgradeEffect
    {
        public string Description { get; }

        public float Value { get; }

        public float GetEffect()
        {
            return Value;
        }
    }
}
