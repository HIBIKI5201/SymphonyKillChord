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
        #region パブリックプロパティ
        /// <summary> 攻撃時に無視するレイヤーマスクを取得します。 </summary>
        public LayerMask IgnoreAttackLayer => _ignoreAttackLayer;

        /// <summary> 地面判定における法線の垂直閾値。 </summary>
        public float GroundNormalVerticalThreshold => _groundNormalVerticalThreshold;
        #endregion

        #region 定数
        private const float DEFAULT_GROUND_NORMAL_VERTICAL_THRESHOLD = 0.5f;
        #endregion

        #region インスペクター表示フィールド
        /// <summary> 攻撃が当たらないレイヤーマスク。 </summary>
        [SerializeField, Tooltip("攻撃が当たらないレイヤーマスク。")]
        private LayerMask _ignoreAttackLayer;
        
        /// <summary> 地面判定における法線の垂直閾値。 </summary>
        [SerializeField, Range(0, 1), Tooltip("地面判定における法線の垂直閾値。")]
        private float _groundNormalVerticalThreshold = DEFAULT_GROUND_NORMAL_VERTICAL_THRESHOLD;
        #endregion
    }
}

