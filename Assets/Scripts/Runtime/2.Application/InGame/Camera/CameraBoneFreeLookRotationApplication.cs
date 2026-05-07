using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera
{
    /// <summary>
    ///     カメラのフリー視点での回転制御を担当するクラス。
    /// </summary>
    public sealed class CameraBoneFreeLookRotationApplication
    {
        public CameraBoneFreeLookRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        public void Update(ref Quaternion cameraBoneRotation, in CameraSystemContext context)
        {
            Vector3 euler = cameraBoneRotation.eulerAngles;
            if (euler.x > 180)
            {
                euler.x -= 360;
            }
            float yaw = euler.y + context.Input.x * _parameter.FollowRotationSpeed * context.DeltaTime;
            float pitch = euler.x - context.Input.y * _parameter.FollowRotationSpeed * context.DeltaTime;

            // ピッチ角の制限
            pitch = Mathf.Clamp(pitch, _parameter.PitchRange.x, _parameter.PitchRange.y);

            cameraBoneRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private readonly CameraSystemParameter _parameter;
    }
}
