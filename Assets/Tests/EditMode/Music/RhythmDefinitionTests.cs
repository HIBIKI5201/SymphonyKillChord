using KillChord.Runtime.Domain.InGame.Music;
using NUnit.Framework;
using System;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     BPMから拍・小節の長さと進捗を求める処理を検証します。
    /// </summary>
    public sealed class RhythmDefinitionTests
    {
        /// <summary>
        ///     BPM120では1拍0.5秒・1小節2秒になることを確認します。
        /// </summary>
        [Test]
        public void Constructor_Bpm120_ComputesBeatAndBarLength()
        {
            RhythmDefinition definition = new(RhythmTestData.BPM);

            Assert.That(definition.BeatLength, Is.EqualTo(0.5d).Within(TOLERANCE));
            Assert.That(definition.BarLength, Is.EqualTo(RhythmTestData.BAR_SECONDS).Within(TOLERANCE));
        }

        /// <summary>
        ///     0以下・NaNのBPMを拒否することを確認します。
        /// </summary>
        [TestCase(0d)]
        [TestCase(-1d)]
        [TestCase(double.NaN)]
        public void Constructor_InvalidBpm_Throws(double bpm)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RhythmDefinition(bpm));
        }

        /// <summary>
        ///     曲頭のオフセットを差し引いて拍位置を求め、オフセット前は0になることを確認します。
        /// </summary>
        [Test]
        public void CalculateAccurateBeat_SubtractsOffset()
        {
            RhythmDefinition definition = new(RhythmTestData.BPM, 1d);

            Assert.That(definition.CalculateAccurateBeat(3d), Is.EqualTo(4d).Within(TOLERANCE));
            Assert.That(definition.CalculateAccurateBeat(0.5d), Is.EqualTo(0d));
        }

        /// <summary>
        ///     小節進捗が0〜1に収まることを確認します。
        /// </summary>
        [TestCase(-1d, 0f)]
        [TestCase(1d, 0.5f)]
        [TestCase(5d, 1f)]
        public void CalculateNormalizedBarProgress_ClampsToOneBar(double seconds, float expected)
        {
            RhythmDefinition definition = new(RhythmTestData.BPM);

            Assert.That(definition.CalculateNormalizedBarProgress(seconds), Is.EqualTo(expected).Within(TOLERANCE));
        }

        private const double TOLERANCE = 1e-6d;
    }
}
