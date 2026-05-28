namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の音楽同期予約に使うタイミング情報をまとめた構造体。
    /// </summary>
    public readonly struct EnemyMusicSpec
    {
        public EnemyMusicSpec(byte barFlag, long timeSignature, long targetBeat)
        {
            BarFlag = barFlag;
            TimeSignature = timeSignature;
            TargetBeat = targetBeat;
        }

        public byte BarFlag { get; }
        public long TimeSignature { get; }
        public long TargetBeat { get; }
    }
}
