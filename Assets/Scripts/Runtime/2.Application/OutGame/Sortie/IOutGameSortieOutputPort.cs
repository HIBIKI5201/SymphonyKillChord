namespace KillChord.Runtime.Application.OutGame.Sortie
{
    /// <summary>
    ///     出撃ボタンを押したときの出撃処理の出力ポート。
    /// </summary>
    public interface IOutGameSortieOutputPort
    {
        /// <summary>
        ///     戦闘準備画面の表示を要求する。
        /// </summary>
        /// <param name="targetSceneName"> 表示する戦闘準備画面のシーン名。 </param>
        void ShowBattlePreparationScreen(string targetSceneName);

        /// <summary>
        ///     戦闘準備画面を介さずにバトル開始を要求します。
        /// </summary>
        /// <param name="targetSceneName"> 遷移先シーン名です。 </param>
        void StartBattle(string targetSceneName);

        /// <summary>
        ///    シナリオステージの出撃処理を要求する。
        /// </summary>
        /// <param name="isActive"> シナリオステージの出撃処理を有効にするかどうか。 </param>
        void SetOutGameActiveForScenario(bool isActive);
    }
}
