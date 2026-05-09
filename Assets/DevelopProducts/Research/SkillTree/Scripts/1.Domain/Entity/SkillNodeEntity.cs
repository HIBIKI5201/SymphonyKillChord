namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId,
                               int cost,
                               INodeUnlockEffect[] nodeUnlockEffects, 
                               bool isUnlocked,
                               bool isRoot = false,
                               SkillNodeEntity parent = null)
        {
            SkillNodeIdVO = new SkillNodeIdVo(nodeId);
            UnlockCost = new UnlockCost(cost);
            NodeUnlockEffects = nodeUnlockEffects;
            _isUnlocked = isUnlocked;
            IsRoot = isRoot;
            Parent = parent;
        }
        public SkillNodeIdVo SkillNodeIdVO { get; }
        public UnlockCost UnlockCost { get; }
        public INodeUnlockEffect[] NodeUnlockEffects { get; }
        public bool IsUnlocked => _isUnlocked;
        public bool IsRoot { get; }
        public SkillNodeEntity Parent { get; }
        
        public void Unlock() => _isUnlocked = true;

        public void Lock()
        {
            //  原点ノードだった場合ロックできない
            if (IsRoot) return;
            
            _isUnlocked = false;
        }
        private bool _isUnlocked;
    }
}