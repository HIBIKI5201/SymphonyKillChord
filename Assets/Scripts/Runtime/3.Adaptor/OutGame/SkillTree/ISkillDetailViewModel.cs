namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキル詳細画面のViewModel。
    /// </summary>
    public interface ISkillDetailViewModel
    {
        /// <summary>
        ///     スキル詳細の表示を更新する。
        /// </summary>
        public void Apply(SkillDetailDTO dto);
    }
}
