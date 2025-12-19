using Mock.MusicBattle.Basis;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///     プレイヤーの設定を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(PlayerConfig))]
    public class PlayerConfig : ScriptableObject
    {
        /// <summary> 攻撃時に無視するレイヤーマスクを取得します。 </summary>
        public LayerMask IgnoreAttackLayer => _ignoreAttackLayer;

        /// <summary> 攻撃が当たらないレイヤーマスク。 </summary>
        [SerializeField, Tooltip("攻撃が当たらないレイヤーマスク。")]
        private LayerMask _ignoreAttackLayer;
    }
}
