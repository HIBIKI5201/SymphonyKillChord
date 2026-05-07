using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Camera
{
    /// <summary>
    ///     カメラシステム全体の動作パラメータを保持するドメインクラス。
    /// </summary>
    [System.Serializable]
    public sealed class CameraSystemParameter
    {
        /// <summary>
        ///     カメラシステム全体の動作パラメータを初期化するコンストラクタ。
        /// </summary>
        /// <param name="cameraOffset"> 追従先を中心としたカメラの基本オフセット位置。</param>
        /// <param name="distance"> 追従先からカメラまでの距離。</param>
        /// <param name="followOffsetPower"> 移動中の追従オフセットの強さ。</param>
        /// <param name="followLerpSpeed"> 移動中の追従オフセットの補間速度。</param>
        /// <param name="boneRotateSpeed"> ロックオン時のカメラボーン回転速度。</param>
        /// <param name="lockOnAngleMargin"> ロックオン時のカメラとターゲットの角度差許容範囲。</param>
        /// <param name="followRotationSpeed"> 非ロックオン時のカメラボーン回転速度。</param>
        /// <param name="lockOnLookAtRatio"> ロックオン時のプレイヤーとターゲット位置の補間比率。</param>
        /// <param name="lockOnRotationSpeed"> ロックオン時のカメラ回転速度。</param>
        /// <param name="collisionRadius"> 衝突判定に使用する球の半径。</param>
        /// <param name="collisionLayerMask"> 衝突判定に使用するレイヤーマスク。</param>
        /// <param name="pitchRange"> ピッチ角度の最小値と最大値。</param>
        /// <param name="invertVertical"> 垂直方向の入力を反転するフラグ。</param>
        /// <param name="invertHorizontal"> 水平方向の入力を反転するフラグ。</param>
        public CameraSystemParameter(
            in Vector3 cameraOffset,
            float distance,
            float followOffsetPower,
            float followLerpSpeed,
            float boneRotateSpeed,
            float lockOnAngleMargin,
            float followRotationSpeed,
            float lockOnLookAtRatio,
            float lockOnRotationSpeed,
            float collisionRadius,
            LayerMask collisionLayerMask,
            Vector2 pitchRange,
            bool invertVertical = false,
            bool invertHorizontal = false
            )
        {
            _cameraOffset = cameraOffset;
            _distance = distance;
            _followOffsetPower = followOffsetPower;
            _followLerpSpeed = followLerpSpeed;
            _boneRotateSpeed = boneRotateSpeed;
            _lockOnAngleMargin = lockOnAngleMargin;
            _followRotationSpeed = followRotationSpeed;
            _lockOnLookAtRatio = lockOnLookAtRatio;
            _lockOnRotationSpeed = lockOnRotationSpeed;
            _collisionRadius = collisionRadius;
            _collisionMask = collisionLayerMask;
            _pitchRange = pitchRange;
            _invertVertical = invertVertical;
            _invertHorizontal = invertHorizontal;
        }

        /// <summary> 追従先を中心としたカメラの基本オフセット位置。 </summary>
        public Vector3 Offset => _cameraOffset;

        /// <summary> 追従先からカメラまでの距離。 </summary>
        public float Distance => _distance;

        /// <summary> 移動中の追従オフセットの強さ。 </summary>
        public float FollowOffsetPower => _followOffsetPower;

        /// <summary> 移動中の追従オフセットの補間速度。 </summary>
        public float FollowLerpSpeed => _followLerpSpeed;

        /// <summary> ロックオン時のカメラボーン回転速度。 </summary>
        public float BoneRotateSpeed => _boneRotateSpeed;

        /// <summary> ロックオン時のカメラとターゲットの角度差許容範囲。 </summary>
        public float LockOnAngleMargin => _lockOnAngleMargin;

        /// <summary> 非ロックオン時のカメラボーン回転速度。 </summary>
        public float FollowRotationSpeed => _followRotationSpeed;

        /// <summary> ロックオン時のプレイヤーとターゲット位置の補間比率。 </summary>
        public float LockOnLookAtRatio => _lockOnLookAtRatio;

        /// <summary> ロックオン時のカメラ回転速度。 </summary>
        public float LockOnRotationSpeed => _lockOnRotationSpeed;

        /// <summary> 衝突判定に使用する球の半径。 </summary>
        public float CollisionRadius => _collisionRadius;

        /// <summary> 衝突判定に使用するレイヤーマスク。 </summary>
        public int CollisionMask => _collisionMask;

        /// <summary> ピッチ角度の最小値と最大値。 </summary>
        public Vector2 PitchRange => _pitchRange;

        /// <summary> 垂直方向の入力反転フラグ。 </summary>
        public bool IsInvertVertical => _invertVertical;

        /// <summary> 水平方向の入力反転フラグ。 </summary>
        public bool IsInvertHorizontal => _invertHorizontal;

        [Header("Main")]
        [Tooltip("追従先を中心としたカメラの基本的オフセット位置")]
        [SerializeField] private Vector3 _cameraOffset;
        [Tooltip("追従先からカメラまでの距離")]
        [SerializeField] private float _distance;

        [Header("Follow")]
        [Tooltip("プレイヤー移動中のカメラの追従オフセットの強さ")]
        [SerializeField] private float _followOffsetPower;
        [Tooltip("プレイヤー移動中のカメラの追従オフセットの補間速度")]
        [SerializeField] private float _followLerpSpeed;

        [Header("Bone Rotation")]
        [Tooltip("ロックオン時のカメラボーンの回転速度")]
        [SerializeField] private float _boneRotateSpeed;
        [Tooltip("ロックオン状態でのカメラとターゲットの角度差の許容範囲")]
        [SerializeField] private float _lockOnAngleMargin;
        [Tooltip("非ロックオン時のカメラボーンの回転速度")]
        [SerializeField] private float _followRotationSpeed;

        [Header("Camera Rotation")]
        [Tooltip("ロックオン時のカメラが向けるプレイヤー位置とターゲット位置の補間比率")]
        [Range(0f, 1f)]
        [SerializeField] private float _lockOnLookAtRatio;
        [Tooltip("ロックオン時のカメラの回転速度")]
        [SerializeField] private float _lockOnRotationSpeed;

        [Header("Collision")]
        [Tooltip("カメラの衝突判定に使用する球の半径")]
        [SerializeField] private float _collisionRadius;
        [Tooltip("カメラの衝突判定に使用するレイヤーマスク")]
        [SerializeField] private LayerMask _collisionMask;

        [Header("Limits")]
        [Tooltip("カメラのピッチ角度(x角度)の最小値と最大値")]
        [SerializeField] private Vector2 _pitchRange;

        [Header("Input Invert")]
        [SerializeField, Tooltip("垂直方向の入力を反転するフラグ")]
        private bool _invertVertical;

        [SerializeField, Tooltip("水平方向の入力を反転するフラグ")]
        private bool _invertHorizontal;
    }
}
