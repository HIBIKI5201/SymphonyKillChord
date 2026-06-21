namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を表示する行のViewインタフェース。
    /// </summary>
    public interface ISkillInputProgressRowView
    {
        /// <summary>
        ///     行内の拍子アイコンの状態を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateSteps(SkillInputProgressUpdateDTO dto);
    }
}
