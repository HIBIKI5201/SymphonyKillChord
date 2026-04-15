using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Skill;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Skill
{
    public interface ISkillRepository
    {
        SkillDefinition GetSkill(int id);
    }
}