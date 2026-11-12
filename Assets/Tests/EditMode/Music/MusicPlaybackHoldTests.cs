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
        ///     開始すると、ずれに1小節分が加わり停止状態になることを確認します。
        /// </summary>
        [Test]
        public void Begin_AddsShiftAndHolds()
        {
            MusicPlaybackHold hold = new();

            hold.Begin(HELD_PLAY_TIME, BAR_LENGTH, TIMEOUT);

            Assert.That(hold.IsHolding, Is.True);
            Assert.That(hold.ShiftSeconds, Is.EqualTo(BAR_LENGTH).Within(TOLERANCE));
            Assert.That(hold.HeldPlayTime, Is.EqualTo(HELD_PLAY_TIME).Within(TOLERANCE));
        }

        /// <summary>
        ///     止めた時刻へ届く前は解除せず、届いたときに解除してずれを残すことを確認します。
        /// </summary>
        [Test]
        public void TryRelease_ReleasesWhenPlayTimeReachesHeldTime()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(HELD_PLAY_TIME, BAR_LENGTH, TIMEOUT);

            Assert.That(hold.TryRelease(HELD_PLAY_TIME - BEAT_LENGTH, FRAME), Is.False);
            Assert.That(hold.IsHolding, Is.True);

            Assert.That(hold.TryRelease(HELD_PLAY_TIME, FRAME), Is.True);
            Assert.That(hold.IsHolding, Is.False);
            Assert.That(hold.ShiftSeconds, Is.EqualTo(BAR_LENGTH).Within(TOLERANCE));
        }

        /// <summary>
        ///     実時間の制限を過ぎたら、加えたずれを取り消して解除することを確認します。
        /// </summary>
        [Test]
        public void TryRelease_CancelsShiftAfterTimeout()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(HELD_PLAY_TIME, BAR_LENGTH, TIMEOUT);

            bool isReleased = hold.TryRelease(HELD_PLAY_TIME - BAR_LENGTH, TIMEOUT);

            Assert.That(isReleased, Is.True);
            Assert.That(hold.IsHolding, Is.False);
            Assert.That(hold.ShiftSeconds, Is.EqualTo(0d).Within(TOLERANCE));
        }

        /// <summary>
        ///     ずれは解除後も積み重なり、取り消しは直前に加えた分だけを戻すことを確認します。
        /// </summary>
        [Test]
        public void Cancel_RevertsOnlyLatestShift()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(HELD_PLAY_TIME, BAR_LENGTH, TIMEOUT);
            hold.TryRelease(HELD_PLAY_TIME, FRAME);

            hold.Begin(HELD_PLAY_TIME + BAR_LENGTH, BAR_LENGTH, TIMEOUT);
            hold.Cancel();

            Assert.That(hold.IsHolding, Is.False);
            Assert.That(hold.ShiftSeconds, Is.EqualTo(BAR_LENGTH).Within(TOLERANCE));
        }

        /// <summary>
        ///     初期化でずれと停止状態が消えることを確認します。
        /// </summary>
        [Test]
        public void Reset_ClearsShiftAndHold()
        {
            MusicPlaybackHold hold = new();
            hold.Begin(HELD_PLAY_TIME, BAR_LENGTH, TIMEOUT);

            hold.Reset();

            Assert.That(hold.IsHolding, Is.False);
            Assert.That(hold.ShiftSeconds, Is.EqualTo(0d).Within(TOLERANCE));
        }

        /// <summary>
        ///     止めた時刻までに通過する拍の数を、3・2・1・0の順に返すことを確認します。
        ///     止めた時刻は拍の途中（2拍目の0.25拍後）とします。
        /// </summary>
        [Test]
        public void CalculateRemainingBeatCount_CountsBeatBoundariesUntilHeldTime()
        {
            MusicPlaybackHold hold = new();
            double heldPlayTime = BEAT_LENGTH * 2.25d;
            hold.Begin(heldPlayTime, BAR_LENGTH, TIMEOUT);

            // 1小節前（-2拍目の0.25拍後）は、まだ最初の拍を通過していない。
            Assert.That(hold.CalculateRemainingBeatCount(heldPlayTime - BAR_LENGTH, BEAT_LENGTH, 0d), Is.EqualTo(4));
            Assert.That(hold.CalculateRemainingBeatCount(BEAT_LENGTH * -1d, BEAT_LENGTH, 0d), Is.EqualTo(3));
            Assert.That(hold.CalculateRemainingBeatCount(0d, BEAT_LENGTH, 0d), Is.EqualTo(2));
            Assert.That(hold.CalculateRemainingBeatCount(BEAT_LENGTH, BEAT_LENGTH, 0d), Is.EqualTo(1));
            Assert.That(hold.CalculateRemainingBeatCount(BEAT_LENGTH * 2d, BEAT_LENGTH, 0d), Is.EqualTo(0));
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
        private const double HELD_PLAY_TIME = 10.3d;
        private const double TIMEOUT = 5d;
        private const double FRAME = 0.016d;
        private const double TOLERANCE = 1e-9d;
    }
}
