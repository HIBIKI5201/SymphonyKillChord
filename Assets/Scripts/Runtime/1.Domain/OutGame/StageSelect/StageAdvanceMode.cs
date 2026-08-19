namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     接続元ステージ完了後の進行方法を表す列挙型。
    /// </summary>
    public enum StageAdvanceMode
    {
        /// <summary> ホームでプレイヤーが次のステージを選択する。 </summary>
        ManualSelection,
        /// <summary> ホームでの選択を挟まず次のステージを開始する。 </summary>
        AutoAdvance,
    }
}
