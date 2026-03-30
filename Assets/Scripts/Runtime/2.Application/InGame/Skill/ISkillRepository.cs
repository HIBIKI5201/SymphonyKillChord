using KillChord.Runtime.Domain.InGame.Skill;

namespace KillChord.Runtime.Application.InGame.Skill
{
    public interface ISkillRepository
    {
        SkillDefinition GetSkill(int id);
    }
}