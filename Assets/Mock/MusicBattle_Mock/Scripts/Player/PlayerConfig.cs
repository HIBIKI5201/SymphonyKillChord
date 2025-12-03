using UnityEngine;

namespace Mock.MusicBattle
{
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = "MusicBattle/" + nameof(PlayerConfig))]
    public class PlayerConfig : ScriptableObject
    {
        public LayerMask IgnoreAttackLayer => _ignoreAttackLayer;

        [SerializeField, Tooltip("攻撃が当たらないタグ")]
        private LayerMask _ignoreAttackLayer;
    }
}
