namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     チュートリアルの色指定攻撃に対する判定結果を表示するViewの抽象です。
    /// </summary>
    public interface ITutorialAttackFeedbackView
    {
        /// <summary>
        ///     成立した攻撃が指定色に一致したかどうかを表示します。
        /// </summary>
        /// <param name="isSuccess"> 指定色と一致した場合はtrueです。 </param>
        void ShowFeedback(bool isSuccess);

        /// <summary>
        ///     指定色でジャスト攻撃が成立したことをPerfectとして表示します。
        /// </summary>
        void ShowPerfectFeedback();
    }
}
