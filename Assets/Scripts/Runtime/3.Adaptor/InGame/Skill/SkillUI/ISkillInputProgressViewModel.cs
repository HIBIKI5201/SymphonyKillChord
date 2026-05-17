namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行の表示を仲介するViewModelのインターフェース。
    /// </summary>
    public interface ISkillInputProgressViewModel
    {
        /// <summary>
        ///     スキル入力進行DTOを反映する。
        /// </summary>
        /// <param name="dtos"> スキル入力進行DTO。 </param>
        void Apply(in SkillInputProgressRowDTO dtos);

        /// <summary>
        ///     表示更新前に状態をクリアする。
        /// </summary>
        void Clear();

        /// <summary>
        ///     表示内容を確定する。
        /// </summary>
        void Commit();
    }
}
