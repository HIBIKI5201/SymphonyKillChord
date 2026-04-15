namespace DevelopProducts.BehaviorGraph.Runtime.Utility
{
    public enum CameraLockOnState : byte
    {
        /// <summary>
        /// 操作によって自由にカメラを回せる
        /// </summary>
        Free,

        /// <summary>
        /// システムによって目標へロックオンした状態
        /// </summary>
        LockOnAuto,

        /// <summary>
        /// 操作によって目標へロックオンした状態
        /// </summary>
        LockOnManual,
    }
}
