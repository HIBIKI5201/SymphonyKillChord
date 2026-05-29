using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public interface ISkillTreeRepository
    {
        SkillNodeEntity[] AllSkillNodes { get; }
        int PhaseCount { get; }
        SkillNodeEntity GetNode(int id);
        IReadOnlyList<SkillNodeEntity> GetParentNodes(int id);

        public IReadOnlyList<SkillNodeEntity> GetNodesByPhase(int phase);
    }
}
