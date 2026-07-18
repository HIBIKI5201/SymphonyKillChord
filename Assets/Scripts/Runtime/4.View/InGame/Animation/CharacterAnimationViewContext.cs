using KillChord.Runtime.Adaptor.InGame.Animation;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションに必要なView側依存をまとめる。
    /// </summary>
    public sealed class CharacterAnimationViewContext : ICharacterAnimationViewContext
    {
        /// <summary>
        ///     View側依存を初期化する。
        /// </summary>
        /// <param name="viewModel"> 連続状態を保持するViewModel。 </param>
        /// <param name="signal"> 瞬間イベントを伝達するSignal。 </param>
        public CharacterAnimationViewContext(
            ICharacterAnimationViewModel viewModel,
            ICharacterAnimationSignal signal)
        {
            ViewModel = viewModel;
            Signal = signal;
        }

        /// <summary> 連続状態を保持するViewModelです。 </summary>
        public ICharacterAnimationViewModel ViewModel { get; }

        /// <summary> 瞬間イベントを伝達するSignalです。 </summary>
        public ICharacterAnimationSignal Signal { get; }
    }
}
