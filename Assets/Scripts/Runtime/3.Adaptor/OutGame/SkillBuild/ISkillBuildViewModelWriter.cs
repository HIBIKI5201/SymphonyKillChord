namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の表示状態を更新する出力用 ViewModel インターフェースです。
    /// </summary>
    public interface ISkillBuildViewModelWriter
    {
        /// <summary>
        ///     改造画面の表示状態を更新します。
        /// </summary>
        /// <param name="dto"> 表示更新 DTO。 </param>
        public void Apply(in SkillBuildViewDTO dto);
    }
}
