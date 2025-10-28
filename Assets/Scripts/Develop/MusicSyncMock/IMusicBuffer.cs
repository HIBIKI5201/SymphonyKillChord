namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     ミュージックバッファのインターフェース。
    /// </summary>
    public interface IMusicBuffer
    {
        public double CurrentBpm { get; }
        public double BeatLength { get; }
        public double CurrentBeat { get; }
    }
}