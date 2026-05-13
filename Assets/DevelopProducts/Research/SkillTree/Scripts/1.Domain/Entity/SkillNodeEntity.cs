using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId,
            int cost,
            INodeUnlockEffect[] nodeUnlockEffects,
            IUnlockCondition unlockCondition,
            bool isUnlocked,
            bool isEnable = false,
            bool isOrigin = false)
        {
            SkillNodeIdVO = new SkillNodeId(nodeId);
            UnlockCost = new UnlockCost(cost);
            NodeUnlockEffects = nodeUnlockEffects;
            UnlockCondition = unlockCondition;
            _isUnlocked = isUnlocked;
            IsEnable = isEnable;
            IsOrigin = isOrigin;
        }

        public SkillNodeId SkillNodeIdVO { get; }
        public UnlockCost UnlockCost { get; }
        public INodeUnlockEffect[] NodeUnlockEffects { get; }
        public IUnlockCondition UnlockCondition { get; }
        public bool IsUnlocked => _isUnlocked;
        public bool IsOrigin { get; }
        public bool IsEnable { get; }
        public SkillNodeEntity[] Parents => _parents;

        public void SetParent(SkillNodeEntity[] parents)
        {
            _parents = parents;
        }
        public void Unlock()
        {
            _isUnlocked = true;
        }

        public bool CanLock()
        {
            return _isEnable;
        }

        public void Lock()
        {
            //  原点ノードだった場合ロックできない
            if (IsOrigin) return;

            _isUnlocked = false;
        }
        public void NodeEnable()
        {
            _isEnable = true;
        }
        public void NodeDisable()
        {
            _isEnable = false;
        }
        private bool _isUnlocked;
        private bool _isEnable;
        private SkillNodeEntity[] _parents;
    }
}