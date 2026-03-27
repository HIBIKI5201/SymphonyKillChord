using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class CameraApplication
    {
        public CameraApplication(
            CameraParameter parameter,
            CollisionResolver collisionResolver)
        {
            _parameter = parameter;
            _collisionResolver = collisionResolver;
        }

        public void RotateCamera(Vector2 input)
        {
            if (IsLockOnMode()) { return; }

            float yaw = input.x * _parameter.CameraRotationSpeed * (_parameter.IsCameraFlipX ? -1f : 1f);
            float pitch = -input.y * _parameter.CameraRotationSpeed;

            _currentPitch = Mathf.Clamp(_currentPitch + pitch, _parameter.PitchRangeMin, _parameter.PitchRangeMax);
            _currentYaw += yaw;
            _currentCameraRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        public void SetLockTarget(Transform lockTarget)
        {
            _lockTarget = lockTarget;
        }

        public void Tick(Transform camera, Transform target, float deltaTime)
        {
            UpdatePitch(camera, target, deltaTime);
            UpdateYaw(camera, target, deltaTime);
        }

        public void DrawLockGizmos(Transform camera)
        {
            if (_lockTarget == null) { return; }

            Vector3 lookDir = _lockTarget.position - camera.position;
            if (lookDir.sqrMagnitude <= 0.0001f) lookDir = camera.forward;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(camera.position, camera.position + lookDir.normalized * 5f);
        }

        private void UpdateYaw(Transform camera, Transform target, float deltaTime)
        {
            Quaternion rotation = IsLockOnMode() ? GetLockYaw(target) : _currentCameraRotation;

            Vector3 idealCameraOffset = rotation * _parameter.CameraOffset;
            Vector3 currentCameraOffset = camera.position - target.position;

            float dampingSpeed = IsLockOnMode() ? _parameter.CameraLockOnFollowDamping : _parameter.CameraPlayerFollowDamping;
            float damping = Mathf.Max(dampingSpeed, 0.0001f);
            float t = 1f - Mathf.Exp(-deltaTime / damping);
            Vector3 slerpedCameraOffset = Vector3.Slerp(currentCameraOffset, idealCameraOffset, t);

            Vector3 preAdjustedCameraPosition = target.position + slerpedCameraOffset;
            Vector3 finalCameraPosition = AdjustCameraForObstacles(target, preAdjustedCameraPosition);

            camera.position = finalCameraPosition;
        }

        private void UpdatePitch(Transform camera, Transform target, float deltaTime)
        {
            Quaternion targetRotation = IsLockOnMode() ? LockTargetPitch(camera) : PlayerPitch(camera, target);

            float dampingSpeed = IsLockOnMode() ? _parameter.CameraLockOnLookAtDamping : _parameter.CameraPlayerLookAtDamping;
            float damping = Mathf.Max(dampingSpeed, 0.0001f);
            float t = 1f - Mathf.Exp(-deltaTime / damping);
            camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, t);
        }

        private Quaternion GetLockYaw(Transform target)
        {
            Vector3 vec = _lockTarget.position - target.position;
            vec.y = 0f;

            if (vec.sqrMagnitude > 0.001f)
            {
                float targetYaw = Mathf.Atan2(vec.x, vec.z) * Mathf.Rad2Deg;
                return Quaternion.Euler(0f, targetYaw, 0f);
            }

            return Quaternion.identity;
        }

        private Vector3 AdjustCameraForObstacles(Transform target, Vector3 cameraPosition)
        {
            Vector3 origin = target.position + _parameter.CameraCollisionOffset;
            Vector3 rayDirection = cameraPosition - origin;
            float distance = rayDirection.magnitude + _parameter.CameraCollisionRadius;

            if (!_collisionResolver(
                    origin,
                    rayDirection.normalized,
                    distance,
                    _parameter.CameraCollisionRadius,
                    target,
                    out Vector3 resolvedPosition))
            {
                return cameraPosition;
            }

            return resolvedPosition;
        }

        private Quaternion LockTargetPitch(Transform camera)
        {
            Vector3 vec = _lockTarget.position - camera.position;
            if (vec.sqrMagnitude <= 0.0001f) { vec = camera.forward; }
            return Quaternion.LookRotation(vec.normalized, Vector3.up);
        }

        private Quaternion PlayerPitch(Transform camera, Transform target)
        {
            Vector3 rotatedLookAtOffset = _currentCameraRotation * _parameter.CameraLookAtOffset;
            Vector3 lookAtPos = target.position + rotatedLookAtOffset;
            return Quaternion.LookRotation(lookAtPos - camera.position);
        }

        private bool IsLockOnMode() => _lockTarget != null;

        public delegate bool CollisionResolver(
            Vector3 origin,
            Vector3 direction,
            float distance,
            float radius,
            Transform ignoreTarget,
            out Vector3 resolvedPosition);

        private readonly CameraParameter _parameter;
        private readonly CollisionResolver _collisionResolver;
        private Transform _lockTarget;
        private float _currentYaw;
        private float _currentPitch;
        private Quaternion _currentCameraRotation = Quaternion.identity;
    }
}
