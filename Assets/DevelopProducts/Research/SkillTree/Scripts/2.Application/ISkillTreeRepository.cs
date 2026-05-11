using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface ISkillTreeRepository
    {
        SkillNodeEntity GetNode(SkillNodeIdVo id);
        IReadOnlyList<SkillNodeEntity> GetParentNodes(SkillNodeIdVo id);
    }
}
