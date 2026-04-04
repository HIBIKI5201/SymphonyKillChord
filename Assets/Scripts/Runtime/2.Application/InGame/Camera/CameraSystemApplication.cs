using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラシステム全体の更新ロジックを統合して管理するアプリケーション層のクラス。
    /// </summary>
    public sealed class CameraSystemApplication
    {
        public CameraSystemApplication(
            CameraSystemParameter parameter,
            CameraFollowApplication followSystem,
            CameraBoneLockOnRotationApplication boneRotationSystem,
            CameraBoneFreeLookRotationApplication freeLookRotationSystem,
            CameraRotationApplication cameraRotationSystem,
            int collisionMask
        )
        {
            _parameter = parameter;
            _followSystem = followSystem;
            _boneRotationSystem = boneRotationSystem;
            _freeLookRotationSystem = freeLookRotationSystem;
            _cameraRotationSystem = cameraRotationSystem;
            _collisionMask = collisionMask;

            _isDistanceInitialized = false;
        }

        public void Update(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            in Vector2 input,
            bool isLockOn,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
            if (isLockOn)
                _boneRotationSystem.Update(ref _cameraBoneRotation, followPosition, targetPosition, deltaTime);
            else
                _freeLookRotationSystem.Update(ref _cameraBoneRotation, input, deltaTime);

            _followSystem.Update(ref _cameraCenterOffset, followPosition, deltaTime);
            _cameraRotationSystem.Update(isLockOn, ref _cameraRotation, _cameraBoneRotation, targetPosition, followPosition, _cameraPosition, deltaTime);

            Vector3 cameraAnchorPosition = followPosition + _cameraCenterOffset;
            Vector3 idealOffset = _cameraBoneRotation * _parameter.Offset;
            float maxDistance = idealOffset.magnitude;
            Vector3 direction = idealOffset.normalized;

            float distance = GetDistance(cameraAnchorPosition, direction, maxDistance);
            if (!_isDistanceInitialized)
            {
                _distance = distance;
                _isDistanceInitialized = true;
            }
            else
            {
                _distance = Mathf.Lerp(Mathf.Min(_distance, distance), distance, deltaTime * 4);
            }

            _cameraPosition = cameraAnchorPosition + direction * _distance;

            resultRotation = _cameraBoneRotation * _cameraRotation;
            resultPosition = _cameraPosition;
        }


        private float GetDistance(in Vector3 center, in Vector3 direction, float maxDistance)
        {
            if (Physics.SphereCast(center, _parameter.CollisionRadius, direction, out RaycastHit hit, maxDistance, _collisionMask))
            {
                return Mathf.Max(0.1f, hit.distance);
            }
            return maxDistance;
        }

        private readonly CameraSystemParameter _parameter;
        private readonly CameraFollowApplication _followSystem;
        private readonly CameraBoneLockOnRotationApplication _boneRotationSystem;
        private readonly CameraBoneFreeLookRotationApplication _freeLookRotationSystem;
        private readonly CameraRotationApplication _cameraRotationSystem;
        private readonly int _collisionMask;

        private float _distance;
        private Vector3 _cameraCenterOffset;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation = Quaternion.identity;
        private Quaternion _cameraBoneRotation = Quaternion.identity;
        private bool _isDistanceInitialized;
    }
}
