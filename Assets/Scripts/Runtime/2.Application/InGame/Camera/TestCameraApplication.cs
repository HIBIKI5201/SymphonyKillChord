using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class TestCameraApplication
    {
        public TestCameraApplication(
            CameraMovementParameter parameter,
            CameraFollow followSystem,
            CameraBoneRotation boneRotationSystem,
            CameraRotation cameraRotationSystem
        )
        {
            _parameter = parameter;
            _followSystem = followSystem;
            _boneRotationSystem = boneRotationSystem;
            _cameraRotationSystem = cameraRotationSystem;
        }

        public void Update(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            bool isLockOn,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
            if (isLockOn)
                _boneRotationSystem.Update(ref _cameraBoneRotation, followPosition, targetPosition, deltaTime);
            _followSystem.Update(ref _cameraCenterOffset, followPosition, _cameraRotation * Vector3.right, isLockOn, deltaTime);
            _cameraRotationSystem.Update(isLockOn, ref _cameraRotation, _cameraBoneRotation, targetPosition, followPosition, _cameraPosition, deltaTime);

            _cameraPosition = followPosition + _cameraCenterOffset + _cameraBoneRotation * _parameter.Offset;

            resultRotation = _cameraBoneRotation * _cameraRotation;
            resultPosition = _cameraPosition;
        }

        private Vector3 _cameraCenterOffset;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation = Quaternion.identity;
        private Quaternion _cameraBoneRotation = Quaternion.identity;

        private readonly CameraMovementParameter _parameter;
        private readonly CameraFollow _followSystem;
        private readonly CameraBoneRotation _boneRotationSystem;
        private readonly CameraRotation _cameraRotationSystem;
    }
}
