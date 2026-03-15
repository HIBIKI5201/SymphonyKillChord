using UnityEngine;

namespace DevelopProducts.Design.GameMode.InfraStructure
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Scriptable Objects/EnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string Id => _id;
        public string DisplayName => _displayName;

        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
    }
}
