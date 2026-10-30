using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using NUnit.Framework;
using System;

namespace KillChord.Tests.EditMode.Skill
{
    /// <summary>
    ///     スキルの入力パターンと入力履歴の照合を検証します。
    /// </summary>
    public sealed class SkillPatternTests
    {
        /// <summary>
        ///     同じ並びの入力履歴と一致し、並び・長さが違えば一致しないことを確認します。
        /// </summary>
        [Test]
        public void Equals_MatchesSameSequenceOnly()
        {
            SkillPattern pattern = CreatePattern(BeatType.One, BeatType.Two, BeatType.Four);

            Assert.That(pattern.Equals(ToSpan(BeatType.One, BeatType.Two, BeatType.Four)), Is.True);
            Assert.That(pattern.Equals(ToSpan(BeatType.Four, BeatType.Two, BeatType.One)), Is.False);
            Assert.That(pattern.Equals(ToSpan(BeatType.One, BeatType.Two)), Is.False);
        }

        /// <summary>
        ///     逆順照合が、新しい入力から並べた履歴と一致することを確認します。
        /// </summary>
        [Test]
        public void EqualsInReverse_MatchesReversedSequence()
        {
            SkillPattern pattern = CreatePattern(BeatType.One, BeatType.Two, BeatType.Four);

            Assert.That(pattern.EqualsInReverse(ToSpan(BeatType.Four, BeatType.Two, BeatType.One)), Is.True);
            Assert.That(pattern.EqualsInReverse(ToSpan(BeatType.One, BeatType.Two, BeatType.Four)), Is.False);
        }

        /// <summary>
        ///     生成元の配列を後から書き換えても、パターンが変わらないことを確認します。
        /// </summary>
        [Test]
        public void RhythmPattern_CopiesSourceArray()
        {
            BeatType[] source = { BeatType.One, BeatType.Two };
            SkillPattern pattern = new(new RhythmPattern(source));

            source[0] = BeatType.Eight;

            Assert.That(pattern.Equals(ToSpan(BeatType.One, BeatType.Two)), Is.True);
            Assert.That(pattern, Is.EqualTo(CreatePattern(BeatType.One, BeatType.Two)));
        }

        /// <summary>
        ///     入力履歴が容量を超えると古い順に捨て、古い順で取り出せることを確認します。
        /// </summary>
        [Test]
        public void SkillRhythmState_KeepsLatestInputsInOrder()
        {
            SkillRhythmState state = new(2);
            state.Enqueue(BeatType.One, 0f, BattleActionType.Attack);
            state.Enqueue(BeatType.Two, 1f, BattleActionType.Attack);
            state.Enqueue(BeatType.Four, 2f, BattleActionType.Dodge);

            Assert.That(state.Count, Is.EqualTo(2));
            Assert.That(state.LastTiming, Is.EqualTo(2f));
            Assert.That(CreatePattern(BeatType.Two, BeatType.Four).Equals(state.GetHistoryBeatType()), Is.True);
            Assert.That(state.GetHistoryActionType()[1], Is.EqualTo(BattleActionType.Dodge));

            state.Clear();
            Assert.That(state.Count, Is.EqualTo(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SkillRhythmState(0));
        }

        /// <summary>
        ///     拍種の並びからスキルパターンを生成します。
        /// </summary>
        /// <param name="beats"> 拍種の並びです。 </param>
        /// <returns> スキルパターンです。 </returns>
        private static SkillPattern CreatePattern(params BeatType[] beats)
        {
            return new SkillPattern(new RhythmPattern(beats));
        }

        /// <summary>
        ///     拍種の並びを読み取り専用スパンにします。
        /// </summary>
        /// <param name="beats"> 拍種の並びです。 </param>
        /// <returns> 読み取り専用スパンです。 </returns>
        private static ReadOnlySpan<BeatType> ToSpan(params BeatType[] beats)
        {
            return beats;
        }
    }
}
