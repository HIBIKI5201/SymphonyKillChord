using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface ISkillTreeRepository
    {
        SkillNodeEntity GetNode(int id);
        IReadOnlyList<SkillNodeEntity> GetParentNodes(int id);
    }
}
