namespace KillChord.Runtime.Adaptor.InGame.Haptics
{
    /// <summary>
    ///     ゲームパッドの振動再生指示を受け取るViewModelインターフェース。
    /// </summary>
    public interface IGamepadHapticsViewModel
    {
        /// <summary>
        ///     ジャスト成立時の振動を一度だけ再生する。
        /// </summary>
        void PlayJustHitPulse();
    }
}
