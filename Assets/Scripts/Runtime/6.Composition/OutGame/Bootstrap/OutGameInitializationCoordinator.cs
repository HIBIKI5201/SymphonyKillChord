using KillChord.Runtime.Composition.Bootstrap;

namespace KillChord.Runtime.Composition.OutGame.Bootstrap
{
    /// <summary>
    /// アウトゲーム初期化モジュールをフェーズ順に実行します。
    /// </summary>
    public sealed class OutGameInitializationCoordinator
        : InitializationCoordinator<IOutGameInitializationModule>
    {
        /// <summary>
        /// ログに表示するCoordinator名です。
        /// </summary>
        protected override string CoordinatorName => nameof(OutGameInitializationCoordinator);
    }
}
