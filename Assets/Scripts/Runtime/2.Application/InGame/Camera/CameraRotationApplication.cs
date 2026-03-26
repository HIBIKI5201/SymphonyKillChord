using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraRotationApplication
    {
        public CameraRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(
            bool isLockOn,
            ref Quaternion rotation,
            in Quaternion boneRotation,
            in Vector3 targetPosition,
            in Vector3 followPosition,
            in Vector3 cameraPosition,
            float deltaTime
        )
        {
            Quaternion target = Quaternion.identity;
            if (isLockOn)
            {
                Vector3 lerpPosition = Vector3.Lerp(followPosition, targetPosition, _parameter.LockOnLookAtRatio);
                Vector3 dir = lerpPosition - cameraPosition;
                target = Quaternion.Inverse(boneRotation) * Quaternion.LookRotation(dir);
            }
            rotation = Quaternion.Slerp(rotation, target, 1f - Mathf.Exp(-_parameter.LockOnRotationSpeed * deltaTime));
        }

        private readonly CameraSystemParameter _parameter;
    }
}
