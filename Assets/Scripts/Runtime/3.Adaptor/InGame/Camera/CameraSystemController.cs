using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの更新要求をアプリケーション層へ仲介するコントローラークラス。
    /// </summary>
    public sealed class CameraSystemController
    {
        public CameraSystemController(CameraSystemApplication application)
        {
            _application = application;
        }

        public void TryActiveAutoLockOn(in Vector3 currentPosition)
        {
            _application.TryActiveAutoLockOn(currentPosition);
        }

        public void ToggleLockOnState(in Vector3 currentPosition)
        {
            _application.ToggleLockOnState(currentPosition);
        }

        public void Update(
            in Vector3 followPosition,
            in Vector2 input,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
            CameraSystemContext context = new(
                 followPosition,
                 input,
                 deltaTime
            );
            _application.Update(context, out resultRotation, out resultPosition);
        }

        private readonly CameraSystemApplication _application;
    }
}
