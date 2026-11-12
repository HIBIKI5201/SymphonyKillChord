namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     ポーズ画面の文言とミッション欄を更新するためのインターフェース。
    /// </summary>
    public interface IPauseWindowViewModel
    {
        /// <summary>
        ///     ポーズ画面の文言を更新する。
        /// </summary>
        /// <param name="dto"> 選択中の言語の文言。 </param>
        public void ApplyLabels(in PauseWindowLabelDTO dto);

        /// <summary>
        ///     ミッション欄の内容を更新する。
        /// </summary>
        /// <param name="dto"> メインミッションとサブミッションの達成状況。 </param>
        public void ApplyMissions(in PauseMissionDTO dto);
    }
}
