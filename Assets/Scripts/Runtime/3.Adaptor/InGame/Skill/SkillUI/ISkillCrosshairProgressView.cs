namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     クロスヘア上のリズムコマンド表示Viewインタフェース。
    /// </summary>
    public interface ISkillCrosshairProgressView
    {
        /// <summary>
        ///     拍子アイコンの点灯/消灯状態を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateSteps(SkillInputProgressUpdateDTO dto);

        /// <summary>
        ///     表示ON/OFFを切り替える。
        /// </summary>
        /// <param name="visible"> 表示する場合はtrue。 </param>
        public void SetVisible(bool visible);
    }
}
