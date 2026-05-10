using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public interface IAlgorithmService
    {
        IReadOnlyList<SkillNodeEntity> FindPath(
            SkillNodeEntity target,
            SkillTreeEntity tree);
    }
}
