namespace KillChord.Runtime.Utility
{
    /// <summary>
    ///     カメラの更新モードを定義します。
    /// </summary>
    public enum CameraUpdateModeEnum : byte
    {
        /// <summary> 更新を停止します。 </summary>
        Stop = 0,
        /// <summary> Updateフェーズで更新します。 </summary>
        Update = 1,
        /// <summary> FixedUpdateフェーズで更新します。 </summary>
        FixedUpdate = 2,
        /// <summary> LateUpdateフェーズで更新します。 </summary>
        LateUpdate = 3,
    }
}

