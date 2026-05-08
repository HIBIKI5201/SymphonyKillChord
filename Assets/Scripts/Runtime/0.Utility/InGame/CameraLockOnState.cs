namespace KillChord.Runtime.Utility
{
    /// <summary>
    ///     カメラのロックオン状態を表す enum。
    /// </summary>
    public enum CameraLockOnState : byte
    {
        /// <summary> 操作によって自由にカメラを回せる状態。 </summary>
        Free,

        /// <summary> システムによって目標へロックオンした状態。 </summary>
        LockOnAuto,

        /// <summary> 操作によって目標へロックオンした状態。 </summary>
        LockOnManual,
    }
}
