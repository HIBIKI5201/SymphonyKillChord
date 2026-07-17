using System.Collections.Generic;
namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     解放条件が全ての親の場合のアルゴリズムサービス
    /// </summary>
    [System.Serializable]
    public class AllNodeSearchService : IAlgorithmService
    {
        /// <summary>
        ///     解放条件が全ての親の場合の経路探索
        /// </summary>
        /// <param name="target"></param>
        /// <param name="skillTreeRepository"></param>
        /// <returns></returns>
        public PathResult FindPath(SkillNodeEntity target, ISkillTreeRepository skillTreeRepository)
        {
            var allParentNodes = skillTreeRepository.GetParentNodes(target.SkillNodeIdVO.Id);
            int totalCost = 0;
            var totalPath = new List<SkillNodeEntity>();
            foreach (var node in allParentNodes)
            {
                var pathResult = node.AlgorithmService.FindPath(node, skillTreeRepository);
                foreach (var root in pathResult.Path)
                {
                    if(!totalPath.Contains(root))
                    {
                        totalPath.Add(root);
                    }
                }
            }
            totalPath.Add(target);
            foreach (var node in totalPath)
            {
                totalCost += node.UnlockCost.Cost;
            }
            return new PathResult(totalPath, new UnlockCost(totalCost));
        }
    }
}
