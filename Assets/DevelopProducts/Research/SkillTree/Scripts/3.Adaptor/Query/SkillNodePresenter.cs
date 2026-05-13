namespace DevelopProducts.SkillTree
{
    public class SkillNodePresenter
    {
        public SkillNodePresenter(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }
        public void CanUnlock(int nodeId, bool canUnlock)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.Push(new CanUnlockDTO(canUnlock));
            }
        }
        private readonly NodeRegistry _nodeRegistry;
    }
}
