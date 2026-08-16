using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの各種パラメータを設定するためのScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CameraConfig), menuName = "KillChord/InGame/CameraSystemConfig")]
    public sealed class CameraConfig : ScriptableObject
    {
        /// <summary> カメラの基本オフセット。 </summary>
        public Vector3 Offset => _cameraOffset;

        /// <summary> キャラクターモデルの中心オフセット。 </summary>
        public Vector3 CharacterCenterOffset => _characterCenterOffset;

        /// <summary> 通常時のカメラ距離。 </summary>
        public float Distance => _distance;

        /// <summary> 移動追従オフセットの強さ。 </summary>
        public float FollowOffsetPower => _followOffsetPower;

        /// <summary> 移動追従オフセットの補間速度。 </summary>
        public float FollowLerpSpeed => _followLerpSpeed;

        /// <summary> ロックオン時のボーン回転速度。 </summary>
        public float BoneRotateSpeed => _boneRotateSpeed;

        /// <summary> ロックオン時のボーン回転の最小速度。 </summary>
        public float LockOnRotationMinSpeed => _lockOnRotationMinSpeed;

        /// <summary> ロックオン時の最大回転速度へ到達する角度差。 </summary>
        public float LockOnRotationSpeedAngleRange => _lockOnRotationSpeedAngleRange;

        /// <summary> ロックオン時の角度許容範囲。 </summary>
        public float LockOnAngleMargin => _lockOnAngleMargin;

        /// <summary> フリールック時の回転速度。 </summary>
        public float FollowRotationSpeed => _followRotationSpeed;

        /// <summary> 非ロックオン時、移動入力の x 成分でカメラの yaw を回転する速度。 </summary>
        public float MoveFollowRotationSpeed => _moveFollowRotationSpeed;

        /// <summary> 視点入力中に移動入力による yaw 回転を無効にするしきい値。 </summary>
        public float MoveFollowIdleLookThreshold => _moveFollowIdleLookThreshold;

        /// <summary> ロックオン注視点の補間比率。 </summary>
        public float LockOnLookAtRatio => _lockOnLookAtRatio;

        /// <summary> ロックオン時のカメラ回転速度。 </summary>
        public float LockOnRotationSpeed => _lockOnRotationSpeed;

        /// <summary> 自動ロックオンを維持するビューポート内側マージン。 </summary>
        public float LockOnViewportMargin => _lockOnViewportMargin;

        /// <summary> 強い視点操作でオートロックオンを解除する判定時間幅。 </summary>
        public float LockOnBreakWindow => _lockOnBreakWindow;

        /// <summary> 強い視点操作でオートロックオンを解除するしきい値。 </summary>
        public float LockOnBreakThreshold => _lockOnBreakThreshold;

        /// <summary> オートロックオン解除までの非命中猶予時間。 </summary>
        public float AutoLockOnReleaseDelay => _autoLockOnReleaseDelay;

        /// <summary> 画面外の敵へ自動ロックオンした直後に視野外解除を猶予する秒数。 </summary>
        public float AutoLockOnViewportGraceDuration => _autoLockOnViewportGraceDuration;

        /// <summary> 衝突判定半径。 </summary>
        public float CollisionRadius => _collisionRadius;

        /// <summary> 衝突判定レイヤー。 </summary>
        public int CollisionMask => _collisionMask;

        /// <summary> ピッチ角度の制限範囲。 </summary>
        public Vector2 PitchRange => _pitchRange;

        /// <summary> 敵撃破時のカメラシェイク設定。 </summary>
        public CameraShakeParameter EnemyDefeatedShake => _enemyDefeatedShake;

        /// <summary> プレイヤー攻撃時のカメラシェイク設定。 </summary>
        public CameraShakeParameter PlayerAttackShake => _playerAttackShake;

        /// <summary> プレイヤー被弾時のカメラシェイク設定。 </summary>
        public CameraShakeParameter PlayerDamageShake => _playerDamageShake;

        /// <summary> 垂直方向の入力反転フラグ。 </summary>
        public bool IsInvertVertical => _invertVertical;

        /// <summary> 水平方向の入力反転フラグ。 </summary>
        public bool IsInvertHorizontal => _invertHorizontal;

        [Header("Main")]
        [Tooltip("追従先を中心としたカメラの基本的オフセット位置")]
        [SerializeField] private Vector3 _cameraOffset;
        [Tooltip("キャラクターモデルの中心オフセット")]
        [SerializeField] private Vector3 _characterCenterOffset;
        [Tooltip("追従先からカメラまでの距離")]
        [SerializeField] private float _distance = 5f;

        [Header("Follow")]
        [Tooltip("プレイヤー移動中のカメラの追従オフセットの強さ")]
        [SerializeField] private float _followOffsetPower = 2f;
        [Tooltip("プレイヤー移動中のカメラの追従オフセットの補間速度")]
        [SerializeField] private float _followLerpSpeed = 1.0f;

        [Header("Bone Rotation")]
        [Tooltip("ロックオン時のカメラボーンの回転速度")]
        [SerializeField] private float _boneRotateSpeed = 1.2f;
        [Tooltip("ロックオン時のカメラボーンの最小回転速度")]
        [SerializeField] private float _lockOnRotationMinSpeed = 0.35f;
        [Tooltip("ロックオン時の最大回転速度へ到達する角度差")]
        [SerializeField] private float _lockOnRotationSpeedAngleRange = 60f;
        [Tooltip("ロックオン状態でのカメラとターゲットの角度差の許容範囲")]
        [SerializeField] private float _lockOnAngleMargin = 10f;
        [Tooltip("非ロックオン時のカメラボーンの回転速度")]
        [SerializeField] private float _followRotationSpeed = 1.5f;
        [Tooltip("非ロックオン時、移動入力の x 成分でカメラの yaw を回転する速度")]
        [SerializeField] private float _moveFollowRotationSpeed = 90f;
        [Tooltip("視点入力中に移動入力による yaw 回転を無効にするしきい値")]
        [SerializeField] private float _moveFollowIdleLookThreshold = 0.01f;

        [Header("Camera Rotation")]
        [Tooltip("ロックオン時のカメラが向けるプレイヤー位置とターゲット位置の補間比率")]
        [Range(0f, 1f)]
        [SerializeField] private float _lockOnLookAtRatio = 0.5f;
        [Tooltip("ロックオン時のカメラの回転速度")]
        [SerializeField] private float _lockOnRotationSpeed = 2.0f;

        [Header("Lock On View")]
        [Range(0f, 0.5f)]
        [SerializeField, Tooltip("自動ロックオンを維持するビューポート内側マージン。")]
        private float _lockOnViewportMargin = 0.05f;

        [Min(0f)]
        [SerializeField, Tooltip("強い視点操作でオートロックオンを解除する判定時間幅。")]
        private float _lockOnBreakWindow = 0.15f;

        [Min(0f)]
        [SerializeField, Tooltip("強い視点操作でオートロックオンを解除するしきい値。")]
        private float _lockOnBreakThreshold = 25f;

        [Min(0f)]
        [SerializeField, Tooltip("最後に対象へ命中してからオートロックオンを解除するまでの秒数。")]
        private float _autoLockOnReleaseDelay = 3f;

        [Min(0f)]
        [SerializeField, Tooltip("画面外の敵へ自動ロックオンした直後に視野外解除を猶予する秒数。")]
        private float _autoLockOnViewportGraceDuration = 0.35f;

        [Header("Collision")]
        [Tooltip("カメラの衝突判定に使用する球の半径")]
        [SerializeField] private float _collisionRadius = 0.2f;
        [Tooltip("カメラの衝突判定に使用するレイヤーマスク")]
        [SerializeField] private LayerMask _collisionMask;

        [Header("Limits")]
        [Tooltip("カメラのピッチ角度( x 角度)の最小値と最大値")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(-45f, 75f);

        [Header("Shake")]
        [SerializeField, Tooltip("敵を撃破した時のカメラシェイク設定。")]
        private CameraShakeParameter _enemyDefeatedShake = new CameraShakeParameter(0.28f, 0.12f, 0.5f, 22f);

        [SerializeField, Tooltip("プレイヤーが攻撃を実行した時のカメラシェイク設定。")]
        private CameraShakeParameter _playerAttackShake = new CameraShakeParameter(0.12f, 0.04f, 0.25f, 26f);

        [SerializeField, Tooltip("プレイヤーが被弾した時のカメラシェイク設定。")]
        private CameraShakeParameter _playerDamageShake = new CameraShakeParameter(0.35f, 0.18f, 0.8f, 20f);

        [Header("Input Invert")]
        [SerializeField, Tooltip("垂直方向の入力を反転するフラグ。")]
        private bool _invertVertical = false;

        [SerializeField, Tooltip("水平方向の入力を反転するフラグ。")]
        private bool _invertHorizontal = false;
    }
}
