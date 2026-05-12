using System.Linq;

namespace DevelopProducts.SkillTree
{ 
    /// <summary>
    ///     全ての親が解放されていたら解放条件を満たすクラス
    /// </summary>
    public class AllParentCondition : IUnlockConditon
    {
        /// <summary>
        ///     全ての親ノードが解放されていたら解放条件を満たす
        /// </summary>
        /// <param name="skillNodeEntity"></param>
        /// <param name="skillTreeEntity"></param>
        /// <returns></returns>
        public bool IsSatisfied(SkillNodeEntity skillNodeEntity, SkillTreeEntity skillTreeEntity)
        {
            if(skillTreeEntity.GetParents(skillNodeEntity.SkillNodeIdVO).All(parent => parent.IsUnlocked))
            {
                return true;
            }
            return false;
        }
    }
}
