using KillChord.Runtime.Domain.InGame.Battle;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.Battle
{
    /// <summary>
    ///     止めていた時間を除いて進む PausableClock を検証します。
    /// </summary>
    public sealed class PausableClockTests
    {
        /// <summary>
        ///     止めていない間は元の時刻をそのまま返すことを確認します。
        /// </summary>
        [Test]
        public void GetTime_ReturnsSourceTimeWhileRunning()
        {
            PausableClock clock = new();

            Assert.That(clock.GetTime(3f), Is.EqualTo(3f).Within(TOLERANCE));
        }

        /// <summary>
        ///     止めている間は時刻が進まないことを確認します。
        /// </summary>
        [Test]
        public void GetTime_DoesNotAdvanceWhileStopped()
        {
            PausableClock clock = new();
            clock.Stop(2f);

            Assert.That(clock.IsStopped, Is.True);
            Assert.That(clock.GetTime(10f), Is.EqualTo(2f).Within(TOLERANCE));
        }

        /// <summary>
        ///     再開後は止めていた時間を除いて進むことを確認します。
        /// </summary>
        [Test]
        public void GetTime_ExcludesStoppedDurationAfterStart()
        {
            PausableClock clock = new();
            clock.Stop(2f);
            clock.Start(5f);
            clock.Stop(6f);
            clock.Start(8f);

            Assert.That(clock.IsStopped, Is.False);
            Assert.That(clock.GetTime(10f), Is.EqualTo(5f).Within(TOLERANCE));
        }

        /// <summary>
        ///     二重に止めても最初に止めた時刻を保つことを確認します。
        /// </summary>
        [Test]
        public void Stop_IgnoresSecondCall()
        {
            PausableClock clock = new();
            clock.Stop(2f);
            clock.Stop(4f);
            clock.Start(6f);

            Assert.That(clock.GetTime(6f), Is.EqualTo(2f).Within(TOLERANCE));
        }

        private const float TOLERANCE = 1e-5f;
    }
}
