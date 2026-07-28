using KillChord.Runtime.Composition.Bootstrap;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
    /// <summary>
    /// インゲーム初期化モジュールをフェーズ順に実行します。
    /// </summary>
    public sealed class InGameInitializationCoordinator
        : InitializationCoordinator<IInGameInitializationModule>
    {
        /// <summary>
        /// ログに表示するCoordinator名です。
        /// </summary>
        protected override string CoordinatorName => nameof(InGameInitializationCoordinator);
    }
}
