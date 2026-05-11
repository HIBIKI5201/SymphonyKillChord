using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public interface IAlgorithmService
    {
        PathResult FindPath(
            SkillNodeEntity target,
            SkillTreeEntity tree);
    }
}
