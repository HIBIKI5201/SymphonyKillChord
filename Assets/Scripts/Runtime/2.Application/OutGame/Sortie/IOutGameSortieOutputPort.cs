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
    }
}
