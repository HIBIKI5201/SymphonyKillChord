using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class PowerUp : IParameterUpgradeEffect
    {
        public float GetEffect()
        {
            return _healthMultiplier;
        }
        [SerializeField] private string _description;
        [SerializeField, Range(0,100)] private float _healthMultiplier;
    }
}
