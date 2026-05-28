namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージの状態を表す列挙型。
    ///     ステージの状態によって、ステージ選択画面での表示が異なるため、判別に使う。
    /// </summary>
    public enum StageStatus
    {
        /// <summary> ステージがロックされている状態。 </summary>
        Locked,
        /// <summary> ステージが解放されている状態。 </summary>
        Unlocked,
        /// <summary> ステージがクリアされている状態。 </summary>
        Cleared,
    }
}
