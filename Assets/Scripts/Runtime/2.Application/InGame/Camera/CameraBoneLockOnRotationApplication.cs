using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラのロックオン時の回転制御を担当するクラス。
    /// </summary>
    public sealed class CameraBoneLockOnRotationApplication
    {
        public CameraBoneLockOnRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Quaternion cameraBoneRotation, in CameraSystemContext context, in Vector3 TargetPosition)
        {
            Vector3 followDir = TargetPosition - context.FollowPosition;
            followDir.y = 0;
            if (followDir.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            Vector3 cameraDir = cameraBoneRotation * Vector3.forward;
            cameraDir.y = 0;
            if (cameraDir.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            float angle = Vector3.Angle(followDir, cameraDir);

            float lockOnAngleMargin = _parameter.LockOnAngleMargin;

            if (angle < lockOnAngleMargin)
            {
                return;
            }
            float crossY = Vector3.Cross(followDir, cameraDir).y;

            Quaternion target = Quaternion.LookRotation(followDir, Vector3.up) * Quaternion.Euler(0, (crossY <= 0) ? -lockOnAngleMargin : lockOnAngleMargin, 0);

            cameraBoneRotation = Quaternion.Slerp(cameraBoneRotation, target, 1f - Mathf.Exp(-_parameter.BoneRotateSpeed * context.DeltaTime));
        }

        private readonly CameraSystemParameter _parameter;
    }
}
