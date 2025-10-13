namespace Mock.MusicSyncMock
{
    public interface IMusicBuffer
    {
        public long CurrentBpm { get; }
        public long BeatLength { get; }
        public long CurrentBeat { get; }
    }
}