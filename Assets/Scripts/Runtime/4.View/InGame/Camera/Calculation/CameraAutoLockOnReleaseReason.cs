namespace KillChord.Runtime.View.InGame.Camera
{
    /// <summary>
    ///     オートロックオンを解除する理由。
    /// </summary>
    public enum CameraAutoLockOnReleaseReason : byte
    {
        /// <summary> 解除しない。 </summary>
        None,

        /// <summary> 一定時間、対象へ命中しなかった。 </summary>
        Idle,

        /// <summary> 対象が有効ビューポートの外へ出た。 </summary>
        OutOfViewport,

        /// <summary> 強い視点操作を行った。 </summary>
        StrongLook,
    }
}
