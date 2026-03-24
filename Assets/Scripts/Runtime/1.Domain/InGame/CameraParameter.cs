using UnityEngine;

namespace KillChord.Runtime.Domain
{
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

        public Vector3 CameraOffset { get; }
        public Vector3 CameraLookAtOffset { get; }
        public float CameraPlayerFollowDamping { get; }
        public float CameraPlayerLookAtDamping { get; }
        public float CameraLockOnFollowDamping { get; }
        public float CameraLockOnLookAtDamping { get; }
        public float CameraRotationSpeed { get; }
        public float PitchRangeMin { get; }
        public float PitchRangeMax { get; }
        public bool IsCameraFlipX { get; }
        public float CameraCollisionRadius { get; }
        public Vector3 CameraCollisionOffset { get; }
    }
}
