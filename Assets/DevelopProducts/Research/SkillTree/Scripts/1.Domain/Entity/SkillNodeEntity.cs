using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId,
            int cost,
            INodeUnlockEffect[] nodeUnlockEffects,
            bool isUnlocked,
            bool isRoot = false,
            bool isEnable = false)
        {
            SkillNodeIdVO = new SkillNodeIdVo(nodeId);
            UnlockCost = new UnlockCost(cost);
            NodeUnlockEffects = nodeUnlockEffects;
            _isUnlocked = isUnlocked;
            IsRoot = isRoot;
            IsEnable = isEnable;
        }

        public SkillNodeIdVo SkillNodeIdVO { get; }
        public UnlockCost UnlockCost { get; }
        public INodeUnlockEffect[] NodeUnlockEffects { get; }
        public bool IsUnlocked => _isUnlocked;
        public bool IsRoot { get; }
        public bool IsEnable { get; }

        public void Unlock() => _isUnlocked = true;

        public bool CanLock()
        {
            return true;
        }

        public void Lock()
        {
            //  原点ノードだった場合ロックできない
            if (IsRoot) return;

            _isUnlocked = false;
        }

        private bool _isUnlocked;
    }
}