using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Camera
{
    [System.Serializable]
    public sealed class CameraSystemParameter
    {
        public CameraSystemParameter(
            in Vector3 cameraOffset,
            float followOffsetPower,
            float followLerpSpeed,
            float boneRotateSpeed,
            float lockOnAngleMargin,
            float followRotationSpeed,
            float lockOnLookAtRatio,
            float lockOnRotationSpeed,
            float collisionRadius,
            Vector2 pitchRange)
        {
            _cameraOffset = cameraOffset;
            _followOffsetPower = followOffsetPower;
            _followLerpSpeed = followLerpSpeed;
            _boneRotateSpeed = boneRotateSpeed;
            _lockOnAngleMargin = lockOnAngleMargin;
            _followRotationSpeed = followRotationSpeed;
            _lockOnLookAtRatio = lockOnLookAtRatio;
            _lockOnRotationSpeed = lockOnRotationSpeed;
            _collisionRadius = collisionRadius;
            _pitchRange = pitchRange;
        }
        public Vector3 Offset => _cameraOffset;
        public float FollowOffsetPower => _followOffsetPower;
        public float FollowLerpSpeed => _followLerpSpeed;

        public float BoneRotateSpeed => _boneRotateSpeed;
        public float LockOnAngleMargin => _lockOnAngleMargin;
        public float FollowRotationSpeed => _followRotationSpeed;

        public float LockOnLookAtRatio => _lockOnLookAtRatio;
        public float LockOnRotationSpeed => _lockOnRotationSpeed;

        public float CollisionRadius => _collisionRadius;

        public Vector2 PitchRange => _pitchRange;

        [Header("Main")]
        [SerializeField] private Vector3 _cameraOffset;

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

        [Header("Limits")]
        [SerializeField] private Vector2 _pitchRange;

    }
}
