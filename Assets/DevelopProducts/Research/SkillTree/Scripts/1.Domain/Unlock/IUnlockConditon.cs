using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface IUnlockConditon
    {
        bool IsSatisfied(SkillNodeEntity skillNodeEntity, SkillTreeEntity skillTreeEntity);
    }
}
