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

        /// <summary> 小節フラグ。0は現在小節、1は次の小節 </summary>
        public byte BarFlag { get; }
        /// <summary> 小節の拍子 </summary>
        public double TimeSignature { get; }
        /// <summary> 拍目 </summary>
        public double TargetBeat { get; }
    }
}
