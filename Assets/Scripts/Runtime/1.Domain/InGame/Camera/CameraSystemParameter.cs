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
        [SerializeField] private Vector3 _cameraOffset;
        [SerializeField] private float _distance;

        [Header("Follow")]
        [SerializeField] private float _followOffsetPower;
        [SerializeField] private float _followLerpSpeed;

        [Header("Bone Rotation")]
        [SerializeField] private float _boneRotateSpeed;
        [SerializeField] private float _lockOnAngleMargin;
        [SerializeField] private float _followRotationSpeed;

        [Header("Camera Rotation")]
        [Range(0f, 1f)]
        [SerializeField] private float _lockOnLookAtRatio;
        [SerializeField] private float _lockOnRotationSpeed;

        [Header("Collision")]
        [SerializeField] private float _collisionRadius;
        [SerializeField] private LayerMask _collisionMask;

        [Header("Limits")]
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
