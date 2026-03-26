using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public sealed class CameraBoneRotation
    {
        public CameraBoneRotation(CameraMovementParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Quaternion cameraBoneRotation, in Vector3 playerPosition, in Vector3 targetPosition, float deltaTime)
        {
            Vector3 followDir = targetPosition - playerPosition;
            followDir.y = 0;
            followDir.Normalize();

            Vector3 cameraDir = cameraBoneRotation * Vector3.forward;
            cameraDir.y = 0;
            cameraDir.Normalize();

            float angle = Vector3.Angle(followDir, cameraDir);

            float ANGLE = 30;

            if (angle < ANGLE)
            {
                return;
            }
            float crossY = Vector3.Cross(followDir, cameraDir).y;

            Quaternion target = Quaternion.LookRotation(followDir, Vector3.up) * Quaternion.Euler(0, (crossY <= 0) ? -ANGLE : ANGLE, 0);

            cameraBoneRotation = Quaternion.Slerp(cameraBoneRotation, target, 1f - Mathf.Exp(-_parameter.RotateLerpSpeed * deltaTime));
        }

        private readonly CameraMovementParameter _parameter;
    }
}
