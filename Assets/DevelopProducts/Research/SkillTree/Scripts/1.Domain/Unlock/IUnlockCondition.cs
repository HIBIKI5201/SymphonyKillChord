using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface IUnlockCondition
    {
        bool IsSatisfied(SkillNodeEntity skillNodeEntity, SkillTreeEntity skillTreeEntity);
    }
}
