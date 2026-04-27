namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果の適用先のインターフェース。
    /// </summary>
    public interface IScreenTransitionApplicable
    {
        /// <summary>
        ///     画面遷移結果を適用します。
        /// </summary>
        void Apply(in ScreenViewDTO screenViewDTO);
    }
}
