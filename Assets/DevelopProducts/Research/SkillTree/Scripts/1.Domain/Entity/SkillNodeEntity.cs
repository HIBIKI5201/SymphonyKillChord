using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId,
            int cost,
            INodeUnlockEffect[] nodeUnlockEffects,
            IUnlockConditon unlockConditon,
            bool isUnlocked,
            bool isEnable = false,
            bool isOrigin = false)
        {
            SkillNodeIdVO = new SkillNodeIdVo(nodeId);
            UnlockCost = new UnlockCost(cost);
            NodeUnlockEffects = nodeUnlockEffects;
            UnlockConditon = unlockConditon;
            _isUnlocked = isUnlocked;
            IsEnable = isEnable;
            IsOrigin = isOrigin;
        }

        public SkillNodeIdVo SkillNodeIdVO { get; }
        public UnlockCost UnlockCost { get; }
        public INodeUnlockEffect[] NodeUnlockEffects { get; }
        public IUnlockConditon UnlockConditon { get; }
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