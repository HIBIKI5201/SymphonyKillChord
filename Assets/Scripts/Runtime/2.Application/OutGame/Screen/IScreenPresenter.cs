namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     画面遷移結果の出力先インターフェース。
    /// </summary>
    public interface IScreenPresenter
    {
        /// <summary>
        ///     画面遷移結果を出力します。
        /// </summary>
        void Present(ScreenTransitionResult result);
    }
}
