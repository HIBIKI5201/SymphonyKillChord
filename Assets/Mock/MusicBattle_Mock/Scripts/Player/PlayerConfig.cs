using Mock.MusicBattle.Basis;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(PlayerConfig))]
    public class PlayerConfig : ScriptableObject
    {
        public LayerMask IgnoreAttackLayer => _ignoreAttackLayer;

        [SerializeField, Tooltip("攻撃が当たらないタグ")]
        private LayerMask _ignoreAttackLayer;
    }
}
