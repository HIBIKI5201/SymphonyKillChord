namespace DevelopProducts.SkillTree
{
    public class SkillNodePresenter
    {
        public SkillNodePresenter(ISkillTreeRepository skillTreeRepository, NodeRegistry nodeRegistry)
        {
            _skillTreeRepository = skillTreeRepository;
            _nodeRegistry = nodeRegistry;
        }
        public void CanUnlock(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.Push(new CanUnlockDTO(node.IsEnable));
            }
        }
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly NodeRegistry _nodeRegistry;
    }
}
