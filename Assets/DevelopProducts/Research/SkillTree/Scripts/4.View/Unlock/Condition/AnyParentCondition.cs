using System.Linq;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     親のどれかが解放されていたら解放条件を満たすクラス
    /// </summary>
    [System.Serializable]
    public class AnyParentCondition : IUnlockCondition
    {
        /// <summary>
        ///     親のどれかが解放されていたら解放条件を満たす
        /// </summary>
        /// <param name="skillNodeEntity"></param>
        /// <param name="skillTreeEntity"></param>
        /// <returns></returns>
        public bool IsSatisfied(SkillNodeEntity skillNodeEntity, SkillTreeEntity skillTreeEntity)
        {
            if(skillTreeEntity.GetParents(skillNodeEntity.SkillNodeIdVO).Any(parent => parent.IsUnlocked))
            {
                return true;
            }
            return false;
        }
    }
}
