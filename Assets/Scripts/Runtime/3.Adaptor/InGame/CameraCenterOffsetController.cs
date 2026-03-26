using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public sealed class CameraCenterOffsetController
    {
        public CameraCenterOffsetController(
            CameraFollow followSystem,
            CameraBoneRotation boneRotationSystem,
            CameraRotation cameraRotationSystem
        )
        {
            _follow = followSystem;
            _boneRotation = boneRotationSystem;
            _cameraRotation = cameraRotationSystem;
        }
        public void Update(
            ref Vector3 cameraCenterPosition,
            ref Quaternion boneRotation,
            ref Quaternion cameraRotation,
            in Vector3 followPostion,
            in Vector3 targetPosition,
            in Vector3 cameraPosition,
            bool isLockOn,
            float deltaTime
            )
        {
            if (isLockOn)
                _boneRotation.Update(ref boneRotation, followPostion, targetPosition, deltaTime);
            _follow.Update(ref cameraCenterPosition, followPostion, cameraRotation * Vector3.right, isLockOn, deltaTime);
            _cameraRotation.Update(isLockOn, ref cameraRotation, boneRotation, targetPosition, followPostion, cameraPosition, deltaTime);
        }

        private readonly CameraFollow _follow;
        private readonly CameraBoneRotation _boneRotation;
        private readonly CameraRotation _cameraRotation;
    }
}
