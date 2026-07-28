namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面の ViewModel インターフェース。
    /// </summary>
    public interface IStageDetailViewModel
    {
        /// <summary>
        ///     ステージ詳細画面の内容を更新する。
        /// </summary>
        /// <param name="dto"> ステージ詳細データ転送オブジェクト。 </param>
        void Apply(in StageDetailDTO dto);
    }
}
