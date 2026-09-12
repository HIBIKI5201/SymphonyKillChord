namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     入力時に確定した攻撃の拍種とジャスト成否を表示へ渡すDTO。
    /// </summary>
    public readonly ref struct PlayerAttackDto
    {
        /// <summary>
        ///     攻撃成立の表示データを生成する。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した拍種の整数値。 </param>
        /// <param name="isJustHit"> 入力時に確定したジャスト成否。 </param>
        public PlayerAttackDto(int beatCount, bool isJustHit)
        {
            BeatCount = beatCount;
            IsJustHit = isJustHit;
        }

        /// <summary> 入力時に確定した拍種の整数値。 </summary>
        public int BeatCount { get; }

        /// <summary> 入力時に確定したジャスト成否。 </summary>
        public bool IsJustHit { get; }
    }
}
