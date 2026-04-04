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

        public void Update(ref Quaternion cameraBoneRotation, Vector2 input, float deltaTime)
        {
            Vector3 euler = cameraBoneRotation.eulerAngles;
            if (euler.x > 180) euler.x -= 360;

            float yaw = euler.y + input.x * _parameter.FollowRotationSpeed * deltaTime;
            float pitch = euler.x - input.y * _parameter.FollowRotationSpeed * deltaTime;

            // ピッチ角の制限
            pitch = Mathf.Clamp(pitch, _parameter.PitchRange.x, _parameter.PitchRange.y);

            cameraBoneRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private readonly CameraSystemParameter _parameter;
    }
}
