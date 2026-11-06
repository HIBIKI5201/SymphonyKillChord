using KillChord.Runtime.Domain.InGame.Music;
using NUnit.Framework;
using System;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     小節進捗から拍種とジャストを判定する処理を検証します。
    /// </summary>
    public sealed class RhythmJudgmentDefinitionTests
    {
        /// <summary>
        ///     入力タイムアウトが2小節であることを確認します（2026-10-14 ディレクター決定）。
        /// </summary>
        [Test]
        public void TimeoutBarCount_IsTwoBars()
        {
            Assert.That(RhythmJudgmentDefinition.TIMEOUT_BAR_COUNT, Is.EqualTo(2f));
            Assert.That(RhythmTestData.CreateJudgmentDefinition().TimeoutBarCount, Is.EqualTo(2f));
        }

        /// <summary>
        ///     判定範囲に応じた拍種を返すことを確認します。
        /// </summary>
        [TestCase(0.1f, BeatType.Four)]
        [TestCase(0.5f, BeatType.Two)]
        [TestCase(1f, BeatType.One)]
        public void TryResolveBeatType_ReturnsRangeBeatType(float progress, BeatType expected)
        {
            RhythmJudgmentDefinition definition = RhythmTestData.CreateJudgmentDefinition();

            Assert.That(definition.TryResolveBeatType(progress, out BeatType beatType), Is.True);
            Assert.That(beatType, Is.EqualTo(expected));
        }

        /// <summary>
        ///     ジャスト範囲は開始を含み終了を含まず、小節末尾を越えても判定できることを確認します。
        /// </summary>
        [TestCase(0.2f, true, BeatType.Four)]
        [TestCase(0.3f, false, default(BeatType))]
        [TestCase(0.5f, true, BeatType.Two)]
        [TestCase(1.02f, true, BeatType.One)]
        [TestCase(1.05f, false, default(BeatType))]
        public void TryResolveJustBeatType_UsesHalfOpenRange(float progress, bool expectedJust, BeatType expected)
        {
            RhythmJudgmentDefinition definition = RhythmTestData.CreateJudgmentDefinition();

            bool isJust = definition.TryResolveJustBeatType(progress, out BeatType beatType);

            Assert.That(isJust, Is.EqualTo(expectedJust));
            if (expectedJust)
            {
                Assert.That(beatType, Is.EqualTo(expected));
            }
        }

        /// <summary>
        ///     不正な判定範囲を拒否することを確認します。
        /// </summary>
        [Test]
        public void RhythmJudgmentRange_InvalidRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RhythmJudgmentRange(BeatType.One, 0.5f, 0.4f, 0.1f, 0.2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RhythmJudgmentRange(BeatType.One, 0f, 1f, 0.5f, 0.5f));
            Assert.Throws<ArgumentNullException>(() => new RhythmJudgmentDefinition(null));
        }
    }
}
