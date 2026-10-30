using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Persistent.Music;
using NUnit.Framework;
using System;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     予約の実行時刻と予告タイミングの計算を検証します。
    /// </summary>
    public sealed class MusicTimingCalculatorTests
    {
        /// <summary>
        ///     「次の小節の3拍目」が、次の小節頭＋2拍の時刻になることを確認します。
        /// </summary>
        [Test]
        public void CalculateExecutionTime_NextBarThirdBeat()
        {
            RhythmDefinition rhythm = new(RhythmTestData.BPM, 0.25d);
            ExecuteRequestTiming timing = new(1, new Beat(4d, 3d));

            // 5.5拍目は2小節目（0始まりで1）なので、対象は3小節目（2）。
            double executeTime = MusicTimingCalculator.CalculateExecutionTime(rhythm, timing, 5.5d);

            Assert.That(executeTime, Is.EqualTo(0.25d + 2d * RhythmTestData.BAR_SECONDS + 1d).Within(TOLERANCE));
        }

        /// <summary>
        ///     拍子を超える拍数の予約を拒否することを確認します。
        /// </summary>
        [Test]
        public void CalculateExecutionTime_CountOverSignature_Throws()
        {
            RhythmDefinition rhythm = new(RhythmTestData.BPM);
            ExecuteRequestTiming timing = new(0, new Beat(4d, 5d));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => MusicTimingCalculator.CalculateExecutionTime(rhythm, timing, 0d));
        }

        /// <summary>
        ///     予約時刻への進捗が区間の前で0、途中で比例、到達後は1になることを確認します。
        /// </summary>
        [TestCase(2d, 1d, 0f)]
        [TestCase(0.25d, 1d, 0.75f)]
        [TestCase(-1d, 1d, 1f)]
        [TestCase(0.5d, 0d, 0f)]
        [TestCase(double.NaN, 1d, 0f)]
        public void CalculateNormalizedApproach(double remaining, double lead, float expected)
        {
            Assert.That(
                MusicTimingCalculator.CalculateNormalizedApproach(remaining, lead),
                Is.EqualTo(expected).Within(TOLERANCE));
        }

        /// <summary>
        ///     同じ小節内で遡れる場合は小節フラグを変えないことを確認します。
        /// </summary>
        [Test]
        public void TryCreateLeadTiming_WithinBar()
        {
            MusicSyncSpec spec = new(0, 4d, 3d);

            Assert.That(MusicTimingCalculator.TryCreateLeadTiming(spec, 1d, out MusicSyncSpec lead), Is.True);
            Assert.That(lead.BarFlag, Is.EqualTo(0));
            Assert.That(lead.TargetBeat, Is.EqualTo(2d).Within(TOLERANCE));
        }

        /// <summary>
        ///     小節頭を跨ぐ場合は前の小節へ繰り下げ、現在より前になる場合は予約できないことを確認します。
        /// </summary>
        [Test]
        public void TryCreateLeadTiming_CrossesBarBoundary()
        {
            MusicSyncSpec nextBar = new(1, 4d, 1d);
            Assert.That(MusicTimingCalculator.TryCreateLeadTiming(nextBar, 1d, out MusicSyncSpec lead), Is.True);
            Assert.That(lead.BarFlag, Is.EqualTo(0));
            Assert.That(lead.TargetBeat, Is.EqualTo(4d).Within(TOLERANCE));

            MusicSyncSpec currentBar = new(0, 4d, 1d);
            Assert.That(MusicTimingCalculator.TryCreateLeadTiming(currentBar, 1d, out _), Is.False);
        }

        /// <summary>
        ///     8拍子・6拍子のタイミングのうち最も近いものへスナップすることを確認します。
        ///     BPM120・オフセット0.25秒なので、1/8小節は0.25秒、1/6小節は約0.333秒です。
        /// </summary>
        [TestCase(0.25d + 0.27d, 0.25d + 0.25d)]
        [TestCase(0.25d + 0.32d, 0.25d + 2d / 6d)]
        [TestCase(0.25d + 1.04d, 0.25d + 1d)]
        [TestCase(0.25d + 1.95d, 0.25d + 2d)]
        public void QuantizePlayTime_SnapsToNearestEightOrSixDivision(double playTime, double expected)
        {
            RhythmDefinition rhythm = new(RhythmTestData.BPM, 0.25d);

            Assert.That(MusicTimingCalculator.QuantizePlayTime(rhythm, playTime), Is.EqualTo(expected).Within(TOLERANCE));
        }

        /// <summary>
        ///     最初の小節頭より前のスナップ結果が負にならないことを確認します。
        /// </summary>
        [Test]
        public void QuantizePlayTime_BeforeOffset_IsNotNegative()
        {
            RhythmDefinition rhythm = new(RhythmTestData.BPM, 0.1d);

            Assert.That(MusicTimingCalculator.QuantizePlayTime(rhythm, 0d), Is.EqualTo(0.1d).Within(TOLERANCE));
            Assert.That(MusicTimingCalculator.QuantizePlayTime(rhythm, 0d), Is.GreaterThanOrEqualTo(0d));
        }

        private const double TOLERANCE = 1e-6d;
    }
}
