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
        public void TryActiveAutoLockOn()
        {
            _application.TryActiveAutoLockOn();
        }
        public void ToggleLockOnState()
        {
            _application.ToggleLockOnState();
        }
        public void Update(
            in Vector3 followPosition,
            in Vector3 targetPosition,
            in Vector2 input,
            float deltaTime,
            out Quaternion resultRotation,
            out Vector3 resultPosition
            )
        {
            CameraSystemContext context = new(
                 followPosition,
                 targetPosition,
                 input,
                 deltaTime
            );
            _application.Update(context, out resultRotation, out resultPosition);
        }

        private readonly CameraSystemApplication _application;
    }
}
