using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraBoneLockOnRotationApplication
    {
        public CameraBoneLockOnRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Quaternion cameraBoneRotation, in Vector3 playerPosition, in Vector3 targetPosition, float deltaTime)
        {
            Vector3 followDir = targetPosition - playerPosition;
            followDir.y = 0;
            if (followDir.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            Vector3 cameraDir = cameraBoneRotation * Vector3.forward;
            cameraDir.y = 0;

            float angle = Vector3.Angle(followDir, cameraDir);

            float lockOnAngleMargin = _parameter.LockOnAngleMargin;

            if (angle < lockOnAngleMargin)
            {
                return;
            }
            float crossY = Vector3.Cross(followDir, cameraDir).y;

            Quaternion target = Quaternion.LookRotation(followDir, Vector3.up) * Quaternion.Euler(0, (crossY <= 0) ? -lockOnAngleMargin : lockOnAngleMargin, 0);

            cameraBoneRotation = Quaternion.Slerp(cameraBoneRotation, target, 1f - Mathf.Exp(-_parameter.BoneRotateSpeed * deltaTime));
        }

        private readonly CameraSystemParameter _parameter;
    }
}
