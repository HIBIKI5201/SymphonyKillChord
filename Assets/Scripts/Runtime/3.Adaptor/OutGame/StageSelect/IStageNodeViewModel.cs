namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージノードの ViewModel インターフェース。
    /// </summary>
    public interface IStageNodeViewModel
    {
        /// <summary>
        ///     ステージノードの状態を更新する。
        /// </summary>
        /// <param name="status"> 更新するステージノードの状態。</param>
        void UpdateStatus(StageStatusView status);
    }
}
