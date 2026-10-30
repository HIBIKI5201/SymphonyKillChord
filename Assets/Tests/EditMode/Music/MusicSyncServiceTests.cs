using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.Persistent.Music;
using NUnit.Framework;
using System.Threading;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     入力履歴・タイムアウト・予約実行を管理する MusicSyncService を検証します。
    /// </summary>
    public sealed class MusicSyncServiceTests
    {
        /// <summary>
        ///     テストごとにBPM120・オフセットなしのサービスを生成します。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _service = new MusicSyncService(
                new RhythmDefinition(RhythmTestData.BPM),
                RhythmTestData.CreateJudgmentDefinition());
            _timedOutCount = 0;
            _service.OnRhythmTimedOut += () => _timedOutCount++;
        }

        /// <summary>
        ///     最初の入力の前は1拍・ジャストなしとして扱うことを確認します。
        /// </summary>
        [Test]
        public void GetCurrentBeatType_BeforeFirstInput_IsOneWithoutJust()
        {
            _service.Update(1d);

            Assert.That(_service.GetCurrentBeatType(out bool isJust), Is.EqualTo(BeatType.One));
            Assert.That(isJust, Is.False);
        }

        /// <summary>
        ///     直前の入力からの経過でジャストと拍種を判定することを確認します。
        /// </summary>
        [Test]
        public void GetCurrentBeatType_HalfBarAfterInput_IsJustTwo()
        {
            _service.Update(0d);
            _service.RegisterBattleActionHistory(BattleActionType.Attack, BeatType.One);
            _service.Update(RhythmTestData.BAR_SECONDS * 0.5d);

            Assert.That(_service.GetCurrentBeatType(out bool isJust), Is.EqualTo(BeatType.Two));
            Assert.That(isJust, Is.True);
        }

        /// <summary>
        ///     1拍のジャストは小節末尾でクランプせず、実際の経過で判定することを確認します。
        /// </summary>
        [Test]
        public void GetCurrentBeatType_AfterOneJustWindow_IsNotJust()
        {
            _service.Update(0d);
            _service.RegisterBattleActionHistory(BattleActionType.Attack, BeatType.One);
            _service.Update(RhythmTestData.BAR_SECONDS * 1.5d);

            Assert.That(_service.GetCurrentBeatType(out bool isJust), Is.EqualTo(BeatType.One));
            Assert.That(isJust, Is.False);
        }

        /// <summary>
        ///     拍から遅れて押した入力の基準が曲の拍へクオンタイズされ、次のジャストがずれないことを確認します。
        /// </summary>
        [Test]
        public void RegisterBattleActionHistory_LateInput_QuantizesGaugeStart()
        {
            // 1小節目の頭（2秒）より0.04秒遅れて押す。
            _service.Update(RhythmTestData.BAR_SECONDS + 0.04d);
            _service.RegisterBattleActionHistory(BattleActionType.Attack, BeatType.One);

            Assert.That(_service.GetBeatTypeTiming()[0], Is.EqualTo((float)RhythmTestData.BAR_SECONDS).Within(TOLERANCE));

            // 基準は2秒なので、2秒＋半小節で2拍のジャストの中心になる。
            _service.Update(RhythmTestData.BAR_SECONDS * 1.5d);
            Assert.That(_service.GetBarProgress(), Is.EqualTo(0.5f).Within(TOLERANCE));
            Assert.That(_service.GetCurrentBeatType(out bool isJust), Is.EqualTo(BeatType.Two));
            Assert.That(isJust, Is.True);
        }

        /// <summary>
        ///     拍より早く押した入力は先の拍へクオンタイズされ、基準に届くまで進捗が0のままになることを確認します。
        /// </summary>
        [Test]
        public void RegisterBattleActionHistory_EarlyInput_QuantizesToNextGrid()
        {
            // 2秒の小節頭より0.05秒早く押す。
            _service.Update(RhythmTestData.BAR_SECONDS - 0.05d);
            _service.RegisterBattleActionHistory(BattleActionType.Attack, BeatType.One);

            Assert.That(_service.GetBeatTypeTiming()[0], Is.EqualTo((float)RhythmTestData.BAR_SECONDS).Within(TOLERANCE));
            Assert.That(_service.GetBarProgressUnclamped(), Is.EqualTo(0f));
        }

        /// <summary>
        ///     入力から2小節経つと履歴を破棄してタイムアウトを1回通知することを確認します。
        /// </summary>
        [Test]
        public void Update_TwoBarsAfterInput_TimesOut()
        {
            _service.Update(0d);
            _service.RegisterBattleActionHistory(BattleActionType.Attack, BeatType.One);

            _service.Update(RhythmTestData.BAR_SECONDS * 2d - 0.01d);
            Assert.That(_service.GetHistoryLength(), Is.EqualTo(1));
            Assert.That(_timedOutCount, Is.EqualTo(0));

            _service.Update(RhythmTestData.BAR_SECONDS * 2d);
            Assert.That(_service.GetHistoryLength(), Is.EqualTo(0));
            Assert.That(_timedOutCount, Is.EqualTo(1));

            // 履歴が空のまま周回しても、再び通知はしない。
            _service.Update(RhythmTestData.BAR_SECONDS * 4d);
            Assert.That(_timedOutCount, Is.EqualTo(1));
        }

        /// <summary>
        ///     被弾などによる強制タイムアウトで履歴を破棄し通知すること、入力が無ければ何もしないことを確認します。
        /// </summary>
        [Test]
        public void ForceRhythmTimeout_ClearsHistoryOnlyAfterInput()
        {
            _service.ForceRhythmTimeout();
            Assert.That(_timedOutCount, Is.EqualTo(0));

            _service.Update(0d);
            _service.RegisterBattleActionHistory(BattleActionType.Dodge, BeatType.Two);
            _service.ForceRhythmTimeout();

            Assert.That(_service.GetHistoryLength(), Is.EqualTo(0));
            Assert.That(_timedOutCount, Is.EqualTo(1));
        }

        /// <summary>
        ///     再生位置が巻き戻ったら、通知せずに履歴を破棄することを確認します。
        /// </summary>
        [Test]
        public void Update_Rewind_ResetsWithoutNotification()
        {
            _service.Update(3d);
            _service.RegisterBattleActionHistory(BattleActionType.Attack, BeatType.One);
            _service.Update(1d);

            Assert.That(_service.GetHistoryLength(), Is.EqualTo(0));
            Assert.That(_timedOutCount, Is.EqualTo(0));
        }

        /// <summary>
        ///     予約したアクションが実行時刻になってから1回だけ実行されることを確認します。
        /// </summary>
        [Test]
        public void RegisterAction_RunsAtExecutionTime()
        {
            int runCount = 0;
            _service.Update(0d);

            // 次の小節の1拍目 = 2秒。
            double executeTime = _service.RegisterAction(
                0d,
                new ExecuteRequestTiming(1, new Beat(4d, 1d)),
                () => runCount++,
                CancellationToken.None);
            Assert.That(executeTime, Is.EqualTo(RhythmTestData.BAR_SECONDS).Within(TOLERANCE));

            _service.Update(1.99d);
            Assert.That(runCount, Is.EqualTo(0));

            _service.Update(2d);
            _service.Update(2.5d);
            Assert.That(runCount, Is.EqualTo(1));
        }

        /// <summary>
        ///     キャンセルされた予約は実行しないことを確認します。
        /// </summary>
        [Test]
        public void RegisterAction_Cancelled_IsSkipped()
        {
            int runCount = 0;
            using CancellationTokenSource cancellation = new();
            _service.Update(0d);
            _service.RegisterAction(
                0d,
                new ExecuteRequestTiming(1, new Beat(4d, 1d)),
                () => runCount++,
                cancellation.Token);

            cancellation.Cancel();
            _service.Update(3d);

            Assert.That(runCount, Is.EqualTo(0));
        }

        private const double TOLERANCE = 1e-6d;

        private MusicSyncService _service;
        private int _timedOutCount;
    }
}
