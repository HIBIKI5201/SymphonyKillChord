namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の音楽同期予約に使うタイミング情報をまとめた構造体。
    /// </summary>
    public readonly struct EnemyMusicSpec
    {
        public EnemyMusicSpec(byte barFlag, double timeSignature, double targetBeat)
        {
            BarFlag = barFlag;
            TimeSignature = timeSignature;
            TargetBeat = targetBeat;
        }

        public byte BarFlag { get; }
        public double TimeSignature { get; }
        public double TargetBeat { get; }
    }
}
