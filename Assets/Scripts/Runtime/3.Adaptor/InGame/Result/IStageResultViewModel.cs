namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///    ステージ結果の表示を行うViewModelのインターフェース。
    /// </summary>
    public interface IStageResultViewModel
    {
        /// <summary>
        ///   ステージ結果のDTOを適用する。
        /// </summary>
        /// <param name="dto"> ステージ結果のDTO。 </param>
        public void Apply(in StageResultDTO dto);
    }
}
