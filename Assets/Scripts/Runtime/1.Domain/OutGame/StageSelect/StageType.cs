namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージの種類を表す列挙型。
    ///     ステージの種類によって詳細情報の内容が異なるため、判別に使う。
    /// </summary>
    public enum StageType
    {
        /// <summary> バトルステージ。 </summary>
        Battle,
        /// <summary> シナリオステージ。 </summary>
        Scenario,
    }
}
