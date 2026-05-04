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

        private readonly CameraSystemApplication _application;
    }
}
