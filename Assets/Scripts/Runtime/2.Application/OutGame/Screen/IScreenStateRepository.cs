namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     画面状態リポジトリのインターフェース。
    /// </summary>
    public interface IScreenStateRepository
    {
        /// <summary> 画面遷移状態を取得します。 </summary>
        ScreenTransitionState TransitionState { get; }
    }
}
