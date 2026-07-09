using UnityEngine;

namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     Camera View の計算に必要な1フレーム分の状態。
    /// </summary>
    public readonly struct CameraUpdateFrame
    {
        /// <summary>
        ///     1フレーム分の状態を初期化する。
        /// </summary>
        /// <param name="context"> 1フレーム分の入力データ。</param>
        /// <param name="targetPosition"> ロックオン対象座標。</param>
        /// <param name="isLockOn"> ロックオン中かどうか。</param>
        public CameraUpdateFrame(
            in CameraUpdateContext context,
            in Vector3 targetPosition,
            bool isLockOn)
        {
            Context = context;
            TargetPosition = targetPosition;
            IsLockOn = isLockOn;
        }

        /// <summary> 1フレーム分の入力データ。 </summary>
        public CameraUpdateContext Context { get; }

        /// <summary> ロックオン対象座標。 </summary>
        public Vector3 TargetPosition { get; }

        /// <summary> ロックオン中かどうか。 </summary>
        public bool IsLockOn { get; }
    }
}
