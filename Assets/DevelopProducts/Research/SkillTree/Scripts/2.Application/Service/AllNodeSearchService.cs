using System.Collections.Generic;
namespace DevelopProducts.SkillTree
{
    [System.Serializable]
    public class AllNodeSearchService : IAlgorithmService
    {
        public PathResult FindPath(SkillNodeEntity target, SkillTreeEntity tree)
        {
            AnyNodeSearchService anyNodeSearchService = new AnyNodeSearchService();

            var allParentNodes = tree.GetParents(target.SkillNodeIdVO);
            int totalCost = 0;
            var totalPath = new List<SkillNodeEntity>();
            foreach (var node in allParentNodes)
            {
                var pathResult = anyNodeSearchService.FindPath(node, tree);
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
