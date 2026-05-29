using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId,
            int cost,
            IAlgorithmService algorithmService,
            bool isUnlocked,
            bool isEnable = false,
            bool isOrigin = false)
        {
            SkillNodeIdVO = new SkillNodeId(nodeId);
            UnlockCost = new UnlockCost(cost);
            AlgorithmService = algorithmService;
            _isUnlocked = isUnlocked;
            _isEnable = isEnable;
            IsOrigin = isOrigin;
        }

        public SkillNodeId SkillNodeIdVO { get; }
        public UnlockCost UnlockCost { get; }
        public IAlgorithmService AlgorithmService { get; }
        public bool IsUnlocked => _isUnlocked;
        public bool IsOrigin { get; }
        public bool IsEnable => _isEnable;
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