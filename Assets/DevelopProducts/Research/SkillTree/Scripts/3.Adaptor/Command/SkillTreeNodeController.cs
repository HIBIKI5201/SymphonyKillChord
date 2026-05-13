using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public class SkillTreeNodeController
    {
        public SkillTreeNodeController(ISkillTreeRepository skillTreeRepository,
                                       IAlgorithmService algorithmService,
                                       CanUnlockUsecase unlockUsecase,
                                       SkillNodePresenter presenter,
                                       SkillTreeEntity skillTree)
        {
            _skillTreeRepository = skillTreeRepository;
            _algorithmService = algorithmService;
            _unlockUsecase = unlockUsecase;
            _skillTree = skillTree;
        }
        public void CanUnlock(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            var path = _algorithmService.FindPath(node, _skillTree);
            var canUnlock = _unlockUsecase.CheckUnlock(node, _skillTree, path.TotalCost.Cost);
            if(canUnlock)
            {
                _skillsNodePresenter.Unlock(nodeId);
            }
        }
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly IAlgorithmService _algorithmService;
        private readonly CanUnlockUsecase _unlockUsecase;
        private readonly SkillNodePresenter _skillsNodePresenter;
        private readonly SkillTreeEntity _skillTree;
    }
}
