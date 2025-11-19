namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラのアップデートモード。
    /// </summary>
    public enum CameraUpdateModeEnum : byte
    {
        Stop = 0,
        Update = 1,
        FixedUpdate = 2,
        LateUpdate = 3,
    }
}
