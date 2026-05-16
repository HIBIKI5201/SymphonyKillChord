using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class NodeCanUnlockController
    {
        public NodeCanUnlockController(ISkillTreeRepository skillTreeRepository,
                                       CanUnlockUsecase unlockUsecase,
                                       SkillNodePresenter presenter,
                                       SkillTreeEntity skillTree)
        {
            _skillTreeRepository = skillTreeRepository;
            _unlockUsecase = unlockUsecase;
            _skillsNodePresenter = presenter;
            _skillTree = skillTree;
        }
        public void CanUnlock(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            var path = node.AlgorithmService.FindPath(node, _skillTree);
            var canUnlock = _unlockUsecase.CheckUnlock(path.TotalCost.Cost);

            _skillsNodePresenter.CanUnlock(nodeId, canUnlock);
        }
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly CanUnlockUsecase _unlockUsecase;
        private readonly SkillNodePresenter _skillsNodePresenter;
        private readonly SkillTreeEntity _skillTree;
    }
}
