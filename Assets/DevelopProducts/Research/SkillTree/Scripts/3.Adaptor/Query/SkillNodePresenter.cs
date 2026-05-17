namespace DevelopProducts.SkillTree
{
    public class SkillNodePresenter
    {
        public SkillNodePresenter(NodeRegistry nodeRegistry,
            SkillTreeEntity skillTree,
            ISkillTreeRepository skillTreeRepository)
        {
            _nodeRegistry = nodeRegistry;
            _skillTree = skillTree;
            _skillTreeRepository = skillTreeRepository;
        }
        public void CanUnlock(int nodeId, bool canUnlock)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.Check(new CanUnlockDTO(canUnlock));
            }
        }
        public void Unlock(int nodeId, bool isUnlock)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.Unlock(new UnlockDTO(isUnlock));
            }
        }
        public void Lock(int nodeId, bool isLock)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.Lock(new LockDTO(isLock));
            }
        }
        public int GetTotalCost(int nodeId)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                var node = _skillTreeRepository.GetNode(nodeId);
                var pathResult = node.AlgorithmService.FindPath(node, _skillTree);
                return pathResult.TotalCost.Cost;
            }
            return 0;
        }
        private readonly NodeRegistry _nodeRegistry;
        private readonly SkillTreeEntity _skillTree;
        private readonly ISkillTreeRepository _skillTreeRepository;
    }
}
