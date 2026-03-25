using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class CameraCenterOffsetController
    {
        public CameraCenterOffsetController(CameraFollow followSystem, CameraRotation rotationSystem)
        {
            _follow = followSystem;
            _rotation = rotationSystem;
        }
        public void Update(
            ref Vector3 cameraCenterPosition,
            ref Quaternion rotation,
            in Vector3 followPostion,
            in Vector3 cameraRight,
            in Vector3 targetPosition,
            bool isLockOn,
            float deltaTime
            )
        {
            _follow.Update(ref cameraCenterPosition, followPostion, cameraRight, isLockOn, deltaTime);
            if (isLockOn)
                _rotation.Update(ref rotation, followPostion, targetPosition, deltaTime);
        }

        private readonly CameraFollow _follow;
        private readonly CameraRotation _rotation;
    }
}
