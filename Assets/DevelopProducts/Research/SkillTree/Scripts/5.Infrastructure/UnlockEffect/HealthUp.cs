using System;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class HealthUp : IParameterUpgradeEffect
    {
        public float GetEffect()
        {
            return _value;
        }
        [SerializeField] private float _value;
    }
}
