namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     キャラクターアニメーションに必要な依存をまとめるインターフェース。
    /// </summary>
    public interface ICharacterAnimationViewContext
    {
        /// <summary> 連続状態を保持するViewModelです。 </summary>
        public ICharacterAnimationViewModel ViewModel { get; }

        /// <summary> 瞬間イベントを伝達するSignalです。 </summary>
        public ICharacterAnimationSignal Signal { get; }
    }
}
