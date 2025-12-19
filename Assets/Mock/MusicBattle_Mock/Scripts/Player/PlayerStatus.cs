using Mock.MusicBattle.Basis;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///     プレイヤーのステータスを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerStatus), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(PlayerStatus))]
    public class PlayerStatus : ScriptableObject
    {
        /// <summary> 移動速度。 </summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary> 歩行時の加速時間。 </summary>
        public float WalkAccelerationDuration => _walkAccelerationDuration;
        /// <summary> 停止時の加速時間。 </summary>
        public float StopAccelerationDuration => _stopAccelerationDuration;
        /// <summary> 回転減衰。 </summary>
        public float RotationDamping => _rotationDamping;
        /// <summary> 攻撃力。 </summary>
        public float AttackPower => _attackPower;
        /// <summary> 攻撃範囲。 </summary>
        public float AttackRange => _attackRange;
        /// <summary> 最大ヘルス。 </summary>
        public float MaxHealth => _maxHealth;
        /// <summary> 地面判定における法線の垂直閾値。 </summary>
        public float GroundNormalVerticalThreshold => _groundNormalVerticalThreshold;

        /// <summary> 移動速度。 </summary>
        [SerializeField, Tooltip("移動速度。")]
        private float _moveSpeed;
        /// <summary> 歩行時の加速時間。 </summary>
        [SerializeField, Tooltip("歩行時の加速時間。")]
        private float _walkAccelerationDuration;
        /// <summary> 停止時の加速時間。 </summary>
        [SerializeField, Tooltip("停止時の加速時間。")]
        private float _stopAccelerationDuration;
        /// <summary> 回転減衰。 </summary>
        [SerializeField, Tooltip("回転減衰。")]
        private float _rotationDamping;

        /// <summary> 攻撃力。 </summary>
        [SerializeField, Tooltip("攻撃力。")] 
        private float _attackPower;
        /// <summary> 攻撃範囲。 </summary>
        [SerializeField, Tooltip("攻撃範囲。")]
        private float _attackRange;
        /// <summary> 最大ヘルス。 </summary>
        [SerializeField, Tooltip("最大ヘルス。")]
        private float _maxHealth;

        /// <summary> 地面判定における法線の垂直閾値。 </summary>
        [SerializeField, Range(0, 1), Tooltip("地面判定における法線の垂直閾値。")]
        private float _groundNormalVerticalThreshold = 0.5f;
    }
}
