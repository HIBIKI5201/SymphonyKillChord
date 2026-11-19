using KillChord.Runtime.Domain.InGame.Music;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     ポーズ解除のカウントダウン中に音楽時間を止める MusicPlaybackHold を検証します。
    /// </summary>
    public sealed class MusicPlaybackHoldTests
    {
        /// <summary>
        ///     開始すると、再開する時刻を覚えて停止状態になることを確認します。
        /// </summary>
        [Test]
        public void Begin_HoldsUntilResumeTime()
        {
            MusicPlaybackHold hold = new();

            hold.Begin(RESUME_PLAY_TIME, TIMEOUT);

            Assert.That(hold.IsHolding, Is.True);
            Assert.That(hold.ResumePlayTime, Is.EqualTo(RESUME_PLAY_TIME).Within(TOLERANCE));
        }

        /// <summary>
        ///     再開する時刻へ届く前は解除せず、届いたときに解除することを確認します。
        /// </summary>
        [Test]
        public void TryRelease_ReleasesWhenPlayTimeReachesResumeTime()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(RESUME_PLAY_TIME, TIMEOUT);

            Assert.That(hold.TryRelease(RESUME_PLAY_TIME - BEAT_LENGTH, FRAME), Is.False);
            Assert.That(hold.IsHolding, Is.True);

            Assert.That(hold.TryRelease(RESUME_PLAY_TIME, FRAME), Is.True);
            Assert.That(hold.IsHolding, Is.False);
        }

        /// <summary>
        ///     実時間の制限を過ぎたら、届いていなくても解除することを確認します。
        /// </summary>
        [Test]
        public void TryRelease_ReleasesAfterTimeout()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(RESUME_PLAY_TIME, TIMEOUT);

            bool isReleased = hold.TryRelease(RESUME_PLAY_TIME - BAR_LENGTH, TIMEOUT);

            Assert.That(isReleased, Is.True);
            Assert.That(hold.IsHolding, Is.False);
        }

        /// <summary>
        ///     取りやめると停止状態が消え、もう一度開始できることを確認します。
        /// </summary>
        [Test]
        public void Cancel_AllowsBeginAgain()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(RESUME_PLAY_TIME, TIMEOUT);

            hold.Cancel();
            hold.Begin(RESUME_PLAY_TIME + BAR_LENGTH, TIMEOUT);

            Assert.That(hold.IsHolding, Is.True);
            Assert.That(hold.ResumePlayTime, Is.EqualTo(RESUME_PLAY_TIME + BAR_LENGTH).Within(TOLERANCE));
        }

        /// <summary>
        ///     初期化で停止状態が消えることを確認します。
        /// </summary>
        [Test]
        public void Reset_ClearsHold()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(RESUME_PLAY_TIME, TIMEOUT);

            hold.Reset();

            Assert.That(hold.IsHolding, Is.False);
            Assert.That(hold.ResumePlayTime, Is.EqualTo(0d).Within(TOLERANCE));
        }

        /// <summary>
        ///     小節の頭から1小節鳴らすとき、残りの拍の数を4・3・2・1・0の順に返すことを確認します。
        /// </summary>
        [Test]
        public void CalculateRemainingBeatCount_CountsBeatsOfOneBar()
        {
            MusicPlaybackHold hold = new();
            double resumePlayTime = BAR_LENGTH * 3d;
            hold.Begin(resumePlayTime, TIMEOUT);

            double start = resumePlayTime - BAR_LENGTH;
            Assert.That(hold.CalculateRemainingBeatCount(start, BEAT_LENGTH, 0d), Is.EqualTo(4));
            Assert.That(hold.CalculateRemainingBeatCount(start + BEAT_LENGTH, BEAT_LENGTH, 0d), Is.EqualTo(3));
            Assert.That(hold.CalculateRemainingBeatCount(start + BEAT_LENGTH * 2d, BEAT_LENGTH, 0d), Is.EqualTo(2));
            Assert.That(hold.CalculateRemainingBeatCount(start + BEAT_LENGTH * 3d, BEAT_LENGTH, 0d), Is.EqualTo(1));
            Assert.That(hold.CalculateRemainingBeatCount(resumePlayTime, BEAT_LENGTH, 0d), Is.EqualTo(0));
        }

        /// <summary>
        ///     止めていないときは残りの拍数を0とすることを確認します。
        /// </summary>
        [Test]
        public void CalculateRemainingBeatCount_ReturnsZeroWhenNotHolding()
        {
            MusicPlaybackHold hold = new();

            Assert.That(hold.CalculateRemainingBeatCount(0d, BEAT_LENGTH, 0d), Is.EqualTo(0));
        }

        private const double BEAT_LENGTH = 0.5d;
        private const double BAR_LENGTH = BEAT_LENGTH * 4d;
        private const double RESUME_PLAY_TIME = 12d;
        private const double TIMEOUT = 5d;
        private const double FRAME = 0.016d;
        private const double TOLERANCE = 1e-9d;
    }
}
