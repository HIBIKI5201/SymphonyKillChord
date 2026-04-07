using UnityEngine;

namespace KillChord.Runtime.Application
{
    public readonly ref struct CameraSystemContext
    {
        public readonly Vector3 FollowPosition;
        public readonly Vector3 TargetPosition;
        public readonly Vector2 Input;
        public readonly float DeltaTime;
        public readonly bool IsLockOn;
    }
}
