using KillChord.Runtime.Domain.InGame.Skill;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.Skill
{
    /// <summary>
    ///     スキル入力のJustボーナスの計算を検証します。
    /// </summary>
    public sealed class SkillJustBonusTests
    {
        /// <summary>
        ///     全部通常なら基準の割合、全部Justなら100%、その間は直線になることを確認します。
        /// </summary>
        [TestCase(0, 4, 0.5f)]
        [TestCase(2, 4, 0.75f)]
        [TestCase(4, 4, 1f)]
        [TestCase(6, 4, 1f)]
        [TestCase(-1, 4, 0.5f)]
        [TestCase(0, 0, 1f)]
        public void CalculateEffectRate_IsLinearInJustRatio(int justCount, int inputCount, float expected)
        {
            SkillJustBonus bonus = new SkillJustBonus(0.5f, new[] { SkillEffectParameterId.DamageMultiplier });

            float rate = bonus.CalculateEffectRate(justCount, inputCount);

            Assert.That(rate, Is.EqualTo(expected).Within(TOLERANCE));
        }

        /// <summary>
        ///     強くするパラメータにだけ割合を掛け、ほかのパラメータは変えないことを確認します。
        /// </summary>
        [Test]
        public void ApplyTo_ScalesOnlyTargetParameters()
        {
            SkillJustBonus bonus = new SkillJustBonus(0.5f, new[] { SkillEffectParameterId.DamageMultiplier });
            SkillEffectSpec spec = new SkillEffectSpec(
                SkillEffectType.Skill00,
                SkillTargetingType.None,
                new[]
                {
                    new SkillEffectParameter(SkillEffectParameterId.DamageMultiplier, 4d, SkillEffectValueFormat.Number),
                    new SkillEffectParameter(SkillEffectParameterId.DurationSeconds, 5d, SkillEffectValueFormat.Number),
                });

            SkillEffectSpec scaled = bonus.ApplyTo(spec, 0.5f);

            Assert.That(scaled.GetRequiredValue(SkillEffectParameterId.DamageMultiplier), Is.EqualTo(2d).Within(TOLERANCE));
            Assert.That(scaled.GetRequiredValue(SkillEffectParameterId.DurationSeconds), Is.EqualTo(5d).Within(TOLERANCE));
        }

        /// <summary>
        ///     基準の割合が0〜1の外なら例外になることを確認します。
        /// </summary>
        [Test]
        public void Constructor_ThrowsForOutOfRangeBaseRate()
        {
            Assert.That(
                () => new SkillJustBonus(1.5f, new[] { SkillEffectParameterId.DamageMultiplier }),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        private const double TOLERANCE = 1e-5;
    }
}
