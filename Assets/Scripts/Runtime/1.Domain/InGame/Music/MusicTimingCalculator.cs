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
        ///     予約済みの実行時刻へ向かう進捗を、指定した長さの区間で線形に計算する。
        ///     小節位相ではなく絶対時刻を基準とするため、実行時刻が何小節先であっても
        ///     0から1への遷移は必ず1回だけとなる。
        /// </summary>
        /// <param name="remainingSeconds"> 実行時刻までの残り時間（秒）。 </param>
        /// <param name="leadSeconds"> 0から1へ変化させる区間の長さ（秒）。 </param>
        /// <returns>
        ///     0〜1の値。区間に入る前は0で、実行時刻に向かって1へ近づく。
        ///     実行時刻を過ぎた後は1のままとなるため、予約の有無は呼び出し側で判定する。
        /// </returns>
        public static float CalculateNormalizedApproach(double remainingSeconds, double leadSeconds)
        {
            if (double.IsNaN(remainingSeconds)
                || double.IsInfinity(remainingSeconds)
                || double.IsNaN(leadSeconds)
                || double.IsInfinity(leadSeconds)
                || leadSeconds <= 0d)
            {
                return 0f;
            }

            double phase = FULL_PHASE - remainingSeconds / leadSeconds;
            if (phase <= 0d)
            {
                return 0f;
            }

            return phase >= FULL_PHASE ? (float)FULL_PHASE : (float)phase;
        }

        /// <summary>
        ///     ターゲット拍から指定量だけ遡った予告タイミングを求める。
        ///     遡った結果が小節の頭を跨ぐ場合は、小節フラグを繰り下げて前の小節へ割り当てる。
        /// </summary>
        /// <param name="musicSpec"> 基準となるタイミング。 </param>
        /// <param name="leadCount"> 遡る量。拍子と同じ単位で指定する。 </param>
        /// <param name="leadSpec"> 算出された予告タイミング。 </param>
        /// <returns> 予約可能なタイミングを算出できた場合はtrue。 </returns>
        public static bool TryCreateLeadTiming(in MusicSyncSpec musicSpec, double leadCount, out MusicSyncSpec leadSpec)
        {
            leadSpec = default;

            // 拍子が不正な場合は遡り量を決められない。
            if (musicSpec.TimeSignature <= 0d
                || double.IsNaN(musicSpec.TimeSignature)
                || double.IsInfinity(musicSpec.TimeSignature)
                || double.IsNaN(musicSpec.TargetBeat)
                || double.IsInfinity(musicSpec.TargetBeat)
                || double.IsNaN(leadCount)
                || double.IsInfinity(leadCount))
            {
                return false;
            }

            double targetBeat = musicSpec.TargetBeat - leadCount;
            int barFlag = musicSpec.BarFlag;

            // 1拍目より前にある間は、拍子1小節分だけ戻して前の小節へ送る。
            int barsBack = (int)Math.Ceiling((FIRST_BEAT - targetBeat) / musicSpec.TimeSignature);
            if (barsBack > 0)
            {
                targetBeat += barsBack * musicSpec.TimeSignature;
                barFlag -= barsBack;
            }

            // 現在の小節より前や、拍子の範囲外へはみ出す場合は予約できない。
            if (barFlag < 0 || targetBeat < FIRST_BEAT || targetBeat > musicSpec.TimeSignature)
            {
                return false;
            }

            leadSpec = new MusicSyncSpec((byte)barFlag, musicSpec.TimeSignature, targetBeat);
            return true;
        }

        /// <summary> ジャスト間位相の全体長。 </summary>
        private const double FULL_PHASE = 1d;
        /// <summary> 小節内の最初の拍。 </summary>
        private const double FIRST_BEAT = 1d;
    }
}
