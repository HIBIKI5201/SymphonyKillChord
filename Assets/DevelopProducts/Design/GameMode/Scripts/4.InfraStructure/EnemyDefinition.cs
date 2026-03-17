using DevelopProducts.Design.GameMode.Utility;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.InfraStructure
{
    [CreateAssetMenu(fileName = nameof(EnemyDefinition), menuName = Const.CREATE_ASSET_PATH + nameof(EnemyDefinition))]
    public class EnemyDefinition : ScriptableObject
    {
        public string Id => _id;
        public string DisplayName => _displayName;

        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
    }
}
