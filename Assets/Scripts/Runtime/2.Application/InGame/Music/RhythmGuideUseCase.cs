namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     リズムガイドの表示位置を計算するユースケースクラス。
    /// </summary>
    public class RhythmGuideUseCase
    {
        /// <summary>
        ///     インジケーターの正規化された位置を計算する。
        /// </summary>
        /// <param name="barProgress"> 小節内の進捗。1を超える超過分も受け取る。 </param>
        /// <returns> 1を1小節とする0以上の進捗。表示上限はViewのゲージ全長で決める。 </returns>
        public float CalculateIndicatorNormalized(float barProgress)
        {
            if (barProgress <= 0f)
            {
                return 0f;
            }

            // Viewが持つゲージ全長と異なる上限で進捗を切り捨てると、最端へ到達できない。
            // 1小節を超えた経過も渡し、描画位置のクランプはViewへ集約する。
            return barProgress;
        }
    }
}
