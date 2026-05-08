using KillChord.Runtime.Application.InGame.Camera;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Camera
{
    /// <summary>
    ///     カメラシステムの更新要求をアプリケーション層へ仲介するコントローラークラス。
    /// </summary>
    public sealed class CameraSystemController
    {
        /// <summary>
        ///     カメラシステムアプリケーションを受け取り、コントローラーを初期化するコンストラクタ。
        /// </summary>
        /// <param name="application"> カメラシステムアプリケーション。</param>
        public CameraSystemController(CameraSystemApplication application)
        {
            _application = application;
        }

        /// <summary>
        ///     攻撃時のオートロックオン発動をアプリケーション層へ委譲する。
        /// </summary>
        /// <param name="currentPosition"> プレイヤーの現在位置。</param>
        public void TryActiveAutoLockOn(in Vector3 currentPosition)
        {
            _application.TryActiveAutoLockOn(currentPosition);
        }

        /// <summary>
        ///     マニュアルロックオン状態のトグルをアプリケーション層へ委譲する。
        /// </summary>
        /// <param name="currentPosition"> プレイヤーの現在位置。</param>
        public void ToggleLockOnState(in Vector3 currentPosition)
        {
            _application.ToggleLockOnState(currentPosition);
        }

        private readonly CameraSystemApplication _application;
    }
}
