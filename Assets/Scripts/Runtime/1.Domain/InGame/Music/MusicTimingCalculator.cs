using KillChord.Runtime.Utility.Constant;
using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     音楽の実行タイミングを計算する静的クラス。
    /// </summary>
    public static class MusicTimingCalculator
    {
        /// <summary>
        ///     実行時間を計算する。
        /// </summary>
        /// <param name="rhythmDefinition"> リズム定義。 </param>
        /// <param name="executeRequestTiming"> 実行要求タイミング。 </param>
        /// <param name="accurateBeat"> 現在の正確な拍。 </param>
        /// <returns> 計算された実行時間。 </returns>
        public static double CalculateExecutionTime(
            RhythmDefinition rhythmDefinition,
            ExecuteRequestTiming executeRequestTiming,
            double accurateBeat)
        {
            if (executeRequestTiming.Beat.Signature <= 0d
                || executeRequestTiming.Beat.Count <= 0d
                || executeRequestTiming.Beat.Count > executeRequestTiming.Beat.Signature)
            {
                throw new ArgumentOutOfRangeException(nameof(executeRequestTiming));
            }

            double currentBar = Math.Floor(accurateBeat / MusicConstants.STANDARD_BEATS_PER_BAR);
            double targetBar = currentBar + executeRequestTiming.BarFlag;
            double targetBarStartTime = rhythmDefinition.BeatOffsetSeconds + targetBar * rhythmDefinition.BarLength;
            double offsetInBar = rhythmDefinition.BarLength / executeRequestTiming.Beat.Signature * (executeRequestTiming.Beat.Count - 1d);

            return targetBarStartTime + offsetInBar;
        }

        /// <summary>
        ///     次のターゲット拍へ向かう進捗を、指定した長さの区間で線形に計算する。
        ///     小節ごとにターゲット拍が巡ってくることを前提とする。
        /// </summary>
        /// <param name="accurateBeat"> 現在の正確な拍。 </param>
        /// <param name="timeSignature"> 小節の拍子。小節をいくつに分割するかを表す。 </param>
        /// <param name="targetBeat"> ターゲットとなる拍目。1始まり。 </param>
        /// <param name="leadCount"> 0から1へ変化させる区間の長さ。拍子と同じ単位で指定する。 </param>
        /// <returns>
        ///     0〜1の値。区間に入る前は0で、ターゲット拍の直前に1へ近づく。
        ///     ターゲット拍を跨いだ時点で0へ戻り、次の小節の区間まで0のままとなる。
        /// </returns>
        public static float CalculateNormalizedApproach(
            double accurateBeat,
            double timeSignature,
            double targetBeat,
            double leadCount)
        {
            if (double.IsNaN(accurateBeat)
                || double.IsInfinity(accurateBeat)
                || double.IsNaN(targetBeat)
                || double.IsInfinity(targetBeat)
                || targetBeat <= 1d
                || double.IsNaN(leadCount)
                || double.IsInfinity(leadCount)
                || leadCount <= 0d
                || double.IsNaN(timeSignature)
                || double.IsInfinity(timeSignature)
                || timeSignature <= 0d
                || targetBeat > timeSignature)
            {
                return 0f;
            }

            // 拍子1つ分の長さを拍単位で求める。CalculateExecutionTimeと同じ換算。
            double unitBeats = MusicConstants.STANDARD_BEATS_PER_BAR / timeSignature;
            double targetOffsetBeats = unitBeats * (targetBeat - 1d);
            double leadBeats = unitBeats * leadCount;

            // ターゲット拍を小節頭に揃えた位相へ変換する。
            double shiftedBeat = accurateBeat - targetOffsetBeats;
            double cycleBeats = MusicConstants.STANDARD_BEATS_PER_BAR;
            double beatsIntoCycle = shiftedBeat - Math.Floor(shiftedBeat / cycleBeats) * cycleBeats;

            // 次のターゲット拍までの残り拍数。区間より手前ではまだ変化させない。
            double beatsRemaining = cycleBeats - beatsIntoCycle;
            if (beatsRemaining > leadBeats)
            {
                return 0f;
            }

            return (float)(FULL_PHASE - beatsRemaining / leadBeats);
        }

        /// <summary> ジャスト間位相の全体長。 </summary>
        private const double FULL_PHASE = 1d;
    }
}
