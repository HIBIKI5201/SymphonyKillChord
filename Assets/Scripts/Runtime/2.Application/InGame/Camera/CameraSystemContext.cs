using UnityEngine;

namespace KillChord.Runtime.Application
{
    public readonly ref struct CameraSystemContext
    {
        public CameraSystemContext(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            Vector2 input,
            float deltaTime,
            bool isLockOn)
        {
            FollowPosition = followPosition;
            TargetPosition = targetPosition;
            Input = input;
            DeltaTime = deltaTime;
            IsLockOn = isLockOn;
        }

        public readonly Vector3 FollowPosition;
        public readonly Vector3 TargetPosition;
        public readonly Vector2 Input;
        public readonly float DeltaTime;
        public readonly bool IsLockOn;
    }
}
