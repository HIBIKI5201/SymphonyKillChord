using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class NodeVisibleController
    {
        public NodeVisibleController(SkillTreeRepository skillTreeRepository, SkillNodePresenter skillNodePresenter)
        {
            _skillTreeRepository = skillTreeRepository;
            _skillNodePresenter = skillNodePresenter;
        }
        public void VisibleNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            node.NodeEnable();
            _skillNodePresenter.Visible(nodeId, node.IsEnable);
        }
        public void InVisibleNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            node.NodeDisable();
            _skillNodePresenter.Visible(nodeId, node.IsEnable);
        }
        private readonly SkillTreeRepository _skillTreeRepository;
        private readonly SkillNodePresenter _skillNodePresenter;
    }
}
