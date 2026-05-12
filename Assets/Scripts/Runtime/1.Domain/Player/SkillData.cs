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

        public SkillData(
            int id,
            BeatType[] pattern,
            ISkillEffect skillEffect)
        {
            Id = id;
            Pattern = pattern;
            SkillEffect = skillEffect;
        }

        /// <summary>
        ///     SkillDefinitionに変換する。
        /// </summary>
        public SkillDefinition ToSkillDefinition()
        {
            return new SkillDefinition(
                new SkillId(Id),
                new SkillPattern(new(Pattern)),
                SkillEffect);
        }
    }
}