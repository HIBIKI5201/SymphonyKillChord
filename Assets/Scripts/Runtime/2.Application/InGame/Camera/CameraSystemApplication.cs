using KillChord.Runtime.Domain.InGame.Camera;
using KillChord.Runtime.Utility;
using System.Runtime.CompilerServices;
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

            _distance = _parameter.Offset.magnitude;
        }
        public void TryActiveAutoLockOn()
        {
            if (_lockOnState == CameraLockOnState.LockOnManual)
                return;
            _lockOnState = CameraLockOnState.LockOnAuto;
        }
        public void ToggleLockOnState()
        {
            if (_lockOnState == CameraLockOnState.Free)
                _lockOnState = CameraLockOnState.LockOnManual;
            else
                _lockOnState = CameraLockOnState.Free;
        }
        public void Update(in CameraSystemContext context, out Quaternion resultRotation, out Vector3 resultPosition)
        {
            if (_lockOnState == CameraLockOnState.LockOnAuto && context.Input.sqrMagnitude > float.Epsilon)
                _lockOnState = CameraLockOnState.Free;

            bool isLockOn = _lockOnState != CameraLockOnState.Free;
            UpdateCameraBone(context);
            _followSystem.Update(ref _cameraCenterOffset, context);
            _cameraRotationSystem.Update(isLockOn, ref _cameraRotation, _cameraBoneRotation, _previousCameraPosition, context);


            CalculateCameraPlacement(context, out (Vector3 CameraAnchorPosition, Vector3 Direction, float Distance) result);
            UpdateDistance(ref _distance, result.Distance, context.DeltaTime);


            resultPosition = result.CameraAnchorPosition + result.Direction * _distance;
            resultRotation = _cameraBoneRotation * _cameraRotation;

            _previousCameraPosition = resultPosition;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateDistance(ref float currentDistance, float targetDistance, float deltaTime)
        {
            float speed = 4;
            currentDistance = Mathf.Lerp(Mathf.Min(currentDistance, targetDistance), targetDistance, deltaTime * speed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateCameraPlacement(in CameraSystemContext context, out (Vector3 CameraAnchorPosition, Vector3 Direction, float Distance) result)
        {
            result.CameraAnchorPosition = context.FollowPosition + _cameraCenterOffset;

            Vector3 idealOffset = _cameraBoneRotation * _parameter.Offset;
            float maxDistance = idealOffset.magnitude;
            result.Direction = idealOffset / maxDistance;

            result.Distance = GetDistance(result.CameraAnchorPosition, result.Direction, maxDistance);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCameraBone(in CameraSystemContext context)
        {
            if (_lockOnState != CameraLockOnState.Free)
                _boneRotationSystem.Update(ref _cameraBoneRotation, context);
            else
                _freeLookRotationSystem.Update(ref _cameraBoneRotation, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        private Vector3 _previousCameraPosition;
        private Quaternion _cameraRotation = Quaternion.identity;
        private Quaternion _cameraBoneRotation = Quaternion.identity;
        private CameraLockOnState _lockOnState;
    }
}
