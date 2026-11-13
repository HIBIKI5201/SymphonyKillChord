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
        /// <param name="isLockOnRotationSuppressed"> 視点入力により、ロックオンによるカメラ回転を止めているかどうか。</param>
        public CameraUpdateFrame(
            in CameraUpdateContext context,
            in Vector3 targetPosition,
            bool isLockOn,
            bool isLockOnRotationSuppressed)
        {
            Context = context;
            TargetPosition = targetPosition;
            IsLockOn = isLockOn;
            IsLockOnRotationSuppressed = isLockOn && isLockOnRotationSuppressed;
        }

        /// <summary> 1フレーム分の入力データ。 </summary>
        public CameraUpdateContext Context { get; }

        /// <summary> ロックオン対象座標。 </summary>
        public Vector3 TargetPosition { get; }

        /// <summary> ロックオン中かどうか。 </summary>
        public bool IsLockOn { get; }

        /// <summary> 視点入力により、ロックオンによるカメラ回転を止めているかどうか。ロックオン中でなければ常に false。 </summary>
        public bool IsLockOnRotationSuppressed { get; }

        /// <summary> ロックオンによるカメラ回転を今フレームに行うかどうか。 </summary>
        public bool IsLockOnRotationActive => IsLockOn && !IsLockOnRotationSuppressed;
    }
}
