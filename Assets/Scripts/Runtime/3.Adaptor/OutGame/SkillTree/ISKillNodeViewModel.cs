namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードのViewModel。
    /// </summary>
    public interface ISkillNodeViewModel
    {
        /// <summary>
        ///     スキルノードを未開放にする。
        /// </summary>
        public void SetUnlocked();
        /// <summary>
        ///     スキルノードを選択中にする。
        /// </summary>
        public void SetSelected();
        /// <summary>
        ///     スキルノードを未選択にする。
        /// </summary>
        public void SetUnSelected();
    }
}
