namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     敵の音楽同期予約に使うタイミング情報をまとめた構造体。
    /// </summary>
    public readonly struct EnemyMusicSpec
    {
        public EnemyMusicSpec(long barFlag, long timeSignature, long targetBeat)
        {
            BarFlag = barFlag;
            TimeSignature = timeSignature;
            TargetBeat = targetBeat;
        }

        public long BarFlag { get; }
        public long TimeSignature { get; }
        public long TargetBeat { get; }
    }
}
