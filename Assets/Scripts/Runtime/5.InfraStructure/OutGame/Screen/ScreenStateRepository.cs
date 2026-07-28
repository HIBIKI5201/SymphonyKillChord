using KillChord.Runtime.Application.OutGame.Screen;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     画面状態リポジトリクラス。
    /// </summary>
    public sealed class ScreenStateRepository : IScreenStateRepository
    {
        /// <summary> 画面遷移状態を取得します。 </summary>
        public ScreenTransitionState TransitionState { get; } = new();
    }
}
