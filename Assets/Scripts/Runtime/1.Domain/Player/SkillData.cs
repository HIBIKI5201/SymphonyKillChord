using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキルの設定データを保持するドメインクラス。
    /// </summary>
    public class SkillData
    {
        public int Id { get; }
        public BeatType[] Pattern { get; }
        public ISkillEffect SkillEffect { get; }
        public ISkillVisual SkillVisual { get; }

        public SkillData(
            int id,
            BeatType[] pattern,
            ISkillEffect skillEffect,
            ISkillVisual skillVisual)
        {
            Id = id;
            Pattern = pattern;
            SkillEffect = skillEffect;
            SkillVisual = skillVisual;
        }

        /// <summary>
        ///     SkillDefinitionに変換する。
        /// </summary>
        public SkillDefinition ToSkillDefinition()
        {
            return new SkillDefinition(
                new SkillId(Id),
                new SkillPattern(new(Pattern)),
                SkillEffect,
                SkillVisual);
        }
    }
}