using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Camera
{
    /// <summary>
    ///     カメラシステム全体の動作パラメータを保持するドメインクラス。
    /// </summary>
    [System.Serializable]
    public sealed class CameraSystemParameter
    {
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
#if UNITY_STANDALONE_WIN
             bool invertVertical = false,
            bool invertHorizontal = false
#endif
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
#if UNITY_STANDALONE_WIN
            _invertVertical = invertVertical;
            _invertHorizontal = invertHorizontal;
#endif
        }
        public Vector3 Offset => _cameraOffset;
        public float Distance => _distance;
        public float FollowOffsetPower => _followOffsetPower;
        public float FollowLerpSpeed => _followLerpSpeed;

        public float BoneRotateSpeed => _boneRotateSpeed;
        public float LockOnAngleMargin => _lockOnAngleMargin;
        public float FollowRotationSpeed => _followRotationSpeed;

        public float LockOnLookAtRatio => _lockOnLookAtRatio;
        public float LockOnRotationSpeed => _lockOnRotationSpeed;

        public float CollisionRadius => _collisionRadius;
        public int CollisionMask => _collisionMask;

        public Vector2 PitchRange => _pitchRange;
#if UNITY_STANDALONE_WIN
        /// <summary> 垂直方向の入力反転フラグ。 </summary>
        public bool IsInvertVertical => _invertVertical;
        /// <summary> 水平方向の入力反転フラグ。 </summary>
        public bool IsInvertHorizontal => _invertHorizontal;
#endif



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

#if UNITY_STANDALONE_WIN
        [Header("Input Invert")]
        [SerializeField, Tooltip("垂直方向の入力を反転するフラグ")]
        private bool _invertVertical;

        [SerializeField, Tooltip("水平方向の入力を反転するフラグ")]
        private bool _invertHorizontal;
#endif
    }
}
