using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Utility;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    /// <summary>
    ///     プレイヤーのステータスを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerStatus), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(PlayerStatus))]
    public class PlayerStatus : ScriptableObject
    {
        #region パブリックプロパティ
        /// <summary> 移動速度。 </summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary> 歩行時の加速時間。 </summary>
        public float WalkAccelerationDuration => _walkAccelerationDuration;
        /// <summary> 停止時の加速時間。 </summary>
        public float StopAccelerationDuration => _stopAccelerationDuration;
        /// <summary> 回転減衰。 </summary>
        public float RotationDamping => _rotationDamping;
        /// <summary> 回転減衰。 </summary>
        public float DodgeDuration => _dodgeDuration;
        /// <summary> 回転減衰。 </summary>
        public float DodgeSpeed => _dodgeSpeed;
        /// <summary> 攻撃力。 </summary>
        public float AttackPower => _attackPower;
        /// <summary> 攻撃範囲。 </summary>
        public float AttackRange => _attackRange;
        /// <summary> 攻撃時の硬直時間。 </summary>
        public BeatLength PostAttackMoveLockDuration => _postAttackMoveLockDuration;
        /// <summary> 最大ヘルス。 </summary>
        public float MaxHealth => _maxHealth;
        /// <summary> スペシャルアタックパターン群。 </summary>
        public SpecialAttackData[] SpecialAttackDatas => _specialAttackDatas;
        #endregion

        #region インスペクター表示フィールド
        [Header("移動")]
        /// <summary> 移動速度。 </summary>
        [SerializeField, Tooltip("移動速度。"), Min(0)]
        private float _moveSpeed = 1;
        /// <summary> 歩行時の加速時間。 </summary>
        [SerializeField, Tooltip("歩行時の加速時間。"), Min(0)]
        private float _walkAccelerationDuration = 0.1f;
        /// <summary> 停止時の加速時間。 </summary>
        [SerializeField, Tooltip("停止時の加速時間。"), Min(0)]
        private float _stopAccelerationDuration = 0.1f;
        /// <summary> 回転減衰。 </summary>
        [SerializeField, Tooltip("回転減衰。"), Min(0)]
        private float _rotationDamping = 0.01f;

        [Space]

        [SerializeField, Tooltip("回避時間"), Min(0)]
        private float _dodgeDuration = 1;
        [SerializeField, Tooltip("回避速度"), Min(0)]
        private float _dodgeSpeed = 1;

        [Header("攻撃")]
        /// <summary> 攻撃力。 </summary>
        [SerializeField, Tooltip("攻撃力。"), Min(0)]
        private float _attackPower = 10;
        /// <summary> 攻撃範囲。 </summary>
        [SerializeField, Tooltip("攻撃範囲。"), Min(0)]
        private float _attackRange = 10;
        [SerializeField, Tooltip("攻撃時の硬直時間"), Min(0)]
        private BeatLength _postAttackMoveLockDuration = new(4, 1);

        [Header("防御")]
        /// <summary> 体力。 </summary>
        [SerializeField, Tooltip("体力。")]
        private float _maxHealth = 100;

        [Header("音楽同期")]
        /// <summary> スペシャルアタックパターン群。 </summary>
        [SerializeField, Tooltip("スペシャルアタックパターン群。")]
        private SpecialAttackData[] _specialAttackDatas;
        #endregion

        #region デバッグ
        public void Assert()
        {
            // 移動。
            Debug.Assert(0 < _moveSpeed, "移動速度が設定されていません。", this);
            Debug.Assert(0 < _walkAccelerationDuration, "歩行時の加速時間が設定されていません。", this);
            Debug.Assert(0 < _stopAccelerationDuration, "停止時の加速時間が設定されていません。", this);
            Debug.Assert(0 < _rotationDamping, "回転減衰が設定されていません。", this);

            Debug.Assert(0 < _dodgeDuration, "回避時間が設定されていません。", this);
            Debug.Assert(0 < _dodgeSpeed, "回避速度が設定されていません。", this);

            // 攻撃。
            Debug.Assert(0 < _attackPower, "攻撃力が設定されていません。", this);
            Debug.Assert(0 < _attackRange, "攻撃範囲が設定されていません。", this);
            _postAttackMoveLockDuration.Assert(this);

            // 防御
            Debug.Assert(0 < _maxHealth, "体力が設定されていません。", this);

            // 音楽同期。
            Debug.Assert(0 < _specialAttackDatas.Length, "スペシャルアタックパターン群が空です。", this);
            foreach (SpecialAttackData data in _specialAttackDatas) { data.Assert(this); }
        }

        private void OnValidate()
        {
            Assert();
        }
        #endregion
    }
}