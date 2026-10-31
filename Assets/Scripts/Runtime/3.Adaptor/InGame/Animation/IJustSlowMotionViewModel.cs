namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     ジャスト成立時のスローモーション再生指示を受け取るViewModelインターフェース。
    /// </summary>
    public interface IJustSlowMotionViewModel
    {
        /// <summary>
        ///     プレイヤーと敵のアニメーションにスローモーションをかける。
        /// </summary>
        void PlayJustSlowMotion();
    }
}
