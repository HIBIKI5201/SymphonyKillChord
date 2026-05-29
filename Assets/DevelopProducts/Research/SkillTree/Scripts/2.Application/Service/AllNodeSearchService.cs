using System.Collections.Generic;
namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class AllNodeSearchService : IAlgorithmService
    {
        public PathResult FindPath(SkillNodeEntity target, ISkillTreeRepository skillTreeRepository)
        {
            AnyNodeSearchService anyNodeSearchService = new AnyNodeSearchService();

            var allParentNodes = skillTreeRepository.GetParentNodes(target.SkillNodeIdVO.Id);
            int totalCost = 0;
            var totalPath = new List<SkillNodeEntity>();
            foreach (var node in allParentNodes)
            {
                var pathResult = anyNodeSearchService.FindPath(node, skillTreeRepository);
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
