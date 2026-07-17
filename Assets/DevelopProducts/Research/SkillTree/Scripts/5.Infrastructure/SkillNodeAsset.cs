using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードの初期設定Asset
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeAsset", menuName = "DevelopProducts/SkillTree/SkillNodeAsset")]
    public class SkillNodeAsset : ScriptableObject
    {
        /// <summary>Entity(自分自身)</summary>
        public SkillNodeEntity SkillNodeEntity => _skillNodeEntity;
        /// <summary>親のノード</summary>
        public SkillNodeAsset[] Parents => _parents;
        /// <summary>
        ///     Entityに変換
        /// </summary>
        /// <returns></returns>
        public SkillNodeEntity ToDomain()
        {
            _skillNodeEntity = new SkillNodeEntity(_id, _cost, _algorithmService);
            return _skillNodeEntity;
        }
        [Header("ノードを解放した時の報酬")]
        [SerializeReference, SubclassSelector] private IParameterUpgradeEffect[] _nodeUnlockEffets;
        [SerializeReference, SubclassSelector] private ISkillUnlockEffect[] _skillUnlockEffets;

        [Header("解放するにあたっての必要条件")]
        [SerializeReference, SubclassSelector] private IAlgorithmService _algorithmService;

        [SerializeField] private int _id;
        [SerializeField] private SkillNodeAsset[] _parents;
        [SerializeField] private int _cost;

        private SkillNodeEntity _skillNodeEntity;
    }
}
