using KillChord.Runtime.Adaptor.InGame.StageSelect;

namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///     製品固有の終了条件に応じて、リザルト遷移先を差し替えるポリシーです。
    /// </summary>
    public interface IStageResultExitPolicy
    {
        /// <summary>
        ///     通常遷移を専用シーンへの遷移に差し替えるか判定します。
        /// </summary>
        /// <param name="action"> 選択されたリザルト操作です。 </param>
        /// <param name="selectedBattleStageState"> 現在のバトルステージ状態です。 </param>
        /// <param name="destinationSceneName"> 差し替える遷移先シーン名です。 </param>
        /// <returns> 遷移先を差し替える場合はtrueです。 </returns>
        bool TryGetDestinationScene(
            StageResultExitAction action,
            SelectedBattleStageState selectedBattleStageState,
            out string destinationSceneName);
    }
}
