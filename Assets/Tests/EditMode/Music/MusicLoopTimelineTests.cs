using KillChord.Runtime.Domain.InGame.Music;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     ループする曲の再生位置とゲーム側の音楽時間を換算する MusicLoopTimeline を検証します。
    /// </summary>
    public sealed class MusicLoopTimelineTests
    {
        /// <summary>
        ///     ループが6.1小節のとき、ゲーム側は1周を7小節として扱い、2周目の頭が小節の頭にそろうことを確認します。
        /// </summary>
        [Test]
        public void ToGameTime_AlignsSecondLoopToBarHead_WhenLoopIsNotWholeBars()
        {
            MusicLoopTimeline timeline = new(BAR_LENGTH * 6.1d, BAR_LENGTH);

            Assert.That(timeline.GridLoopLengthSeconds, Is.EqualTo(BAR_LENGTH * 7d).Within(TOLERANCE));

            // 2周目の頭（曲の位置6.1小節）は、ゲーム側では7小節目の頭になる。
            Assert.That(timeline.ToGameTime(BAR_LENGTH * 6.1d), Is.EqualTo(BAR_LENGTH * 7d).Within(TOLERANCE));

            // 3周目の1小節目の途中も、ずれが積もらない。
            double songPosition = BAR_LENGTH * 6.1d * 2d + BAR_LENGTH * 0.5d;
            Assert.That(timeline.ToGameTime(songPosition), Is.EqualTo(BAR_LENGTH * 14.5d).Within(TOLERANCE));
        }

        /// <summary>
        ///     1周目の中ではそのままの時刻を返すことを確認します。
        /// </summary>
        [Test]
        public void ToGameTime_ReturnsSamePositionInFirstLoop()
        {
            MusicLoopTimeline timeline = new(BAR_LENGTH * 6.1d, BAR_LENGTH);

            Assert.That(timeline.ToGameTime(BAR_LENGTH * 6.05d), Is.EqualTo(BAR_LENGTH * 6.05d).Within(TOLERANCE));
        }

        /// <summary>
        ///     ループの長さが小節の整数倍よりわずかに短いとき（S00の7.6ms）、1周を16小節として扱うことを確認します。
        /// </summary>
        [Test]
        public void GridLoopLength_RoundsUpWhenLoopIsSlightlyShort()
        {
            double barLength = 60d / 84d * 4d;
            MusicLoopTimeline timeline = new(2193922d / 48000d, barLength);

            Assert.That(timeline.GridLoopLengthSeconds, Is.EqualTo(barLength * 16d).Within(TOLERANCE));
        }

        /// <summary>
        ///     ループの長さが小節の整数倍とほぼ同じとき（S03）、1周を16小節として扱うことを確認します。
        /// </summary>
        [Test]
        public void GridLoopLength_TreatsAlmostWholeBarsAsWhole()
        {
            double barLength = 60d / 84d * 4d;
            MusicLoopTimeline timeline = new(2194286d / 48000d, barLength);

            Assert.That(timeline.GridLoopLengthSeconds, Is.EqualTo(barLength * 16d).Within(TOLERANCE));
        }

        /// <summary>
        ///     ゲーム側の時刻から曲の位置へ戻せること、飛ばした区間は次のループの頭になることを確認します。
        /// </summary>
        [Test]
        public void ToSongPosition_InvertsGameTime()
        {
            double loopLength = BAR_LENGTH * 6.1d;
            MusicLoopTimeline timeline = new(loopLength, BAR_LENGTH);

            Assert.That(timeline.ToSongPosition(BAR_LENGTH * 9d), Is.EqualTo(loopLength + BAR_LENGTH * 2d).Within(TOLERANCE));
            Assert.That(timeline.ToSongPosition(BAR_LENGTH * 6.5d), Is.EqualTo(loopLength).Within(TOLERANCE));
        }

        /// <summary>
        ///     ループの長さが分からないときは、換算せずにそのまま返すことを確認します。
        /// </summary>
        [Test]
        public void ToGameTime_PassesThroughWhenNotLooping()
        {
            MusicLoopTimeline timeline = new(0d, BAR_LENGTH);

            Assert.That(timeline.IsLooping, Is.False);
            Assert.That(timeline.ToGameTime(123.4d), Is.EqualTo(123.4d).Within(TOLERANCE));
            Assert.That(timeline.ToSongPosition(123.4d), Is.EqualTo(123.4d).Within(TOLERANCE));
        }

        private const double BAR_LENGTH = 2d;
        private const double TOLERANCE = 1e-9d;
    }
}
