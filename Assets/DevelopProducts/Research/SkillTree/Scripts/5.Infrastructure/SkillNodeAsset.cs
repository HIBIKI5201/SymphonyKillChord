using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    [CreateAssetMenu(fileName = "SkillNodeAsset", menuName = "DevelopProducts/SkillTree/SkillNodeAsset")]
    public class SkillNodeAsset : ScriptableObject
    {
        public int Id => _id;
        public SkillNodeEntity SkillNodeEntity => _skillNodeEntity;
        public SkillNodeAsset[] Parents => _parents;
        /// <summary>
        ///     Entityに変換
        /// </summary>
        /// <returns></returns>
        public SkillNodeEntity ToDomain()
        {
            _skillNodeEntity = new SkillNodeEntity(_id, _cost, _algorithmService, _isUnlocked, _isEnable);
            return _skillNodeEntity;
        }
        [Header("ノードを解放した時の報酬")]
        [SerializeReference, SubclassSelector] private IParameterUpgradeEffect[] _nodeUnlockEffets;
        [SerializeReference, SubclassSelector] private ISkillUnlockEffect[] _skillUnlockEffets;

        [Header("解放するにあたっての必要条件")]
        [SerializeReference, SubclassSelector] private IAlgorithmService _algorithmService;

        [SerializeField] private int _id;
        [SerializeField] private bool _isUnlocked;
        [SerializeField] private bool _isEnable;
        [SerializeField] private SkillNodeAsset[] _parents;
        [SerializeField] private int _cost;

        private SkillNodeEntity _skillNodeEntity;
    }
}
