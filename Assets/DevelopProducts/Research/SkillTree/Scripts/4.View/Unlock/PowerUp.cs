using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class PowerUp : IParameterUpgradeEffect
    {
        public string Description { get; }
        public float Value { get; }

        [SerializeField] private string _description;
        [SerializeField] private float _healthMultiplier;
    }
}
