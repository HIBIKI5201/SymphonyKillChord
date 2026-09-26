namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     チュートリアル対象拍の攻撃成功演出を表示するViewModelインターフェース。
    /// </summary>
    public interface IRhythmGuideTargetFeedbackViewModel
    {
        /// <summary>
        ///     対象拍の攻撃成功演出を再生する。
        /// </summary>
        void PlayTargetBeatSuccessFeedback();
    }
}
