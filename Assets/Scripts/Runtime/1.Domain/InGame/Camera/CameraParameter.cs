using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Camera
{
    [System.Serializable]
    public sealed class CameraParameter
    {
        public CameraParameter(
            Vector3 cameraOffset,
            Vector3 cameraLookAtOffset,
            float cameraPlayerFollowDamping,
            float cameraPlayerLookAtDamping,
            float cameraLockOnFollowDamping,
            float cameraLockOnLookAtDamping,
            float cameraRotationSpeed,
            float pitchRangeMin,
            float pitchRangeMax,
            bool isCameraFlipX,
            float cameraCollisionRadius,
            Vector3 cameraCollisionOffset)
        {
            CameraOffset = cameraOffset;
            CameraLookAtOffset = cameraLookAtOffset;
            CameraPlayerFollowDamping = cameraPlayerFollowDamping;
            CameraPlayerLookAtDamping = cameraPlayerLookAtDamping;
            CameraLockOnFollowDamping = cameraLockOnFollowDamping;
            CameraLockOnLookAtDamping = cameraLockOnLookAtDamping;
            CameraRotationSpeed = cameraRotationSpeed;
            PitchRangeMin = pitchRangeMin;
            PitchRangeMax = pitchRangeMax;
            IsCameraFlipX = isCameraFlipX;
            CameraCollisionRadius = cameraCollisionRadius;
            CameraCollisionOffset = cameraCollisionOffset;
        }

        [field: SerializeField]
        public Vector3 CameraOffset { get; private set; }

        [field: SerializeField]
        public Vector3 CameraLookAtOffset { get; private set; }
        [field: SerializeField]
        public float CameraPlayerFollowDamping { get; private set; }
        [field: SerializeField]
        public float CameraPlayerLookAtDamping { get; private set; }
        [field: SerializeField]
        public float CameraLockOnFollowDamping { get; private set; }
        [field: SerializeField]
        public float CameraLockOnLookAtDamping { get; private set; }
        [field: SerializeField]
        public float CameraRotationSpeed { get; private set; }
        [field: SerializeField]
        public float PitchRangeMin { get; private set; }
        [field: SerializeField]
        public float PitchRangeMax { get; private set; }
        [field: SerializeField]
        public bool IsCameraFlipX { get; private set; }
        [field: SerializeField]
        public float CameraCollisionRadius { get; private set; }
        [field: SerializeField]
        public Vector3 CameraCollisionOffset { get; private set; }
    }
}
