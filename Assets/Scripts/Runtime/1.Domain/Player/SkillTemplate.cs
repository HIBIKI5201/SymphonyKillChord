using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキルの設定データを保持するドメインクラス。
    /// </summary>
    public class SkillTemplate
    {
        public int Id { get; }
        public BeatType[] Pattern { get; }
        public double CooldownBarRatio { get; }
        public ISkillEffect SkillEffect { get; }
        public string AnimationKey { get; }

        public SkillTemplate(
            int id,
            BeatType[] pattern,
            int cooldownNumerator,
            int cooldownDenomimator,
            ISkillEffect skillEffect,
            string animationKey)
        {
            if (cooldownDenomimator <= 0)
            {
                throw new ArgumentException($"クールダウン時間の分母は0以下では設定できません。");
            }
            if (cooldownNumerator < 0)
            {
                throw new ArgumentException($"クールダウンの分子は0未満では設定できません。");
            }
            Id = id;
            Pattern = pattern;
            CooldownBarRatio = (double)cooldownNumerator / cooldownDenomimator;
            SkillEffect = skillEffect;
            AnimationKey = animationKey;
        }

        /// <summary>
        ///     SkillDefinitionに変換する。
        /// </summary>
        public SkillDefinition ToSkillDefinition(double bpm)
        {
            return new SkillDefinition(
                new SkillId(Id),
                new SkillPattern(new(Pattern)),
                CooldownBarRatio,
                SkillEffect,
                bpm,
                AnimationKey);
        }
    }
}
