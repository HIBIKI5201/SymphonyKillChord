namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     ジャスト成立時のリズムガイド演出を表示するViewModelインターフェース。
    /// </summary>
    public interface IRhythmGuideJustHitFeedbackViewModel
    {
        /// <summary>
        ///     ジャストが成立した拍のジャスト位置をフラッシュさせる。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した攻撃の拍種の整数値。 </param>
        void PlayJustHitFeedback(int beatCount);
    }
}
