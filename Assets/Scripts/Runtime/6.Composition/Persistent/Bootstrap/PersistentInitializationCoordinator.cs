using KillChord.Runtime.Composition.Bootstrap;

namespace KillChord.Runtime.Composition.Persistent.Bootstrap
{
    /// <summary>
    /// Persistent初期化モジュールをフェーズ順に実行します。
    /// </summary>
    public sealed class PersistentInitializationCoordinator
        : InitializationCoordinator<IPersistentInitializationModule>
    {
        /// <summary>
        /// ログに表示するCoordinator名です。
        /// </summary>
        protected override string CoordinatorName => nameof(PersistentInitializationCoordinator);
    }
}
