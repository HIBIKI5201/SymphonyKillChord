using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillNodeAsset", menuName = "DevelopProducts/SkillTree/SkillNodeAsset")]
    public class SkillNodeAsset : ScriptableObject
    {
        public int Id => _id;
        public SkillNodeEntity SkillNodeEntity => _skillNodeEntity;
        public INodeUnlockEffect[] NodeUnlockEffects => _nodeUnlockEffets;
        public SkillNodeAsset[] Parents => _parents;
        public int Cost => _cost;

        public SkillNodeEntity ToDomain()
        {
            _skillNodeEntity = new SkillNodeEntity(_id, _cost, _nodeUnlockEffets, _unlockCondition, _isUnlocked, _isEnable);
            return _skillNodeEntity;
        }
        [SerializeReference, SubclassSelector] private INodeUnlockEffect[] _nodeUnlockEffets;
        [SerializeReference, SubclassSelector] private IUnlockCondition _unlockCondition;

        [SerializeField] private int _id;
        [SerializeField] private bool _isUnlocked;
        [SerializeField] private bool _isEnable;
        [SerializeField] private SkillNodeAsset[] _parents;
        [SerializeField] private int _cost;

        private SkillNodeEntity _skillNodeEntity;
    }
}
