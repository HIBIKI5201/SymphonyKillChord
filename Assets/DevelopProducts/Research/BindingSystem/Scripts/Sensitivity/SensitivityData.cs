using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    [CreateAssetMenu(fileName = "SensitivityData", menuName = "DevelopProducts/SensitivityData")]
    public class SensitivityData : ScriptableObject
    {
        public float SensitivityMultiplier => _sensitivityMultiplier;

        [SerializeField, Range(0.1f, 3f)] private float _sensitivityMultiplier = 1f;
    }
}
