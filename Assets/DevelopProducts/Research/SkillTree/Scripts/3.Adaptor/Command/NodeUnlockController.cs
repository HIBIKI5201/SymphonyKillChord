namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///  ノードを解放するコントローラー
    /// </summary>
    public class NodeUnlockController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unlockUsecase">スキルをアンロックするユースケース</param>
        public NodeUnlockController(UnlockUsecase unlockUsecase,
                                    ISkillTreeRepository skillTreeRepository,
                                    SkillTreeEntity skillTreeEntity,
                                    SkillNodePresenter skillNodePresenter)
        {
            _unlockUsecase = unlockUsecase;
            _skillTreeRepository = skillTreeRepository;
            _skillTreeEntity = skillTreeEntity;
            _skillNodePresenter = skillNodePresenter;
        }
        /// <summary>
        /// ノードを解放する
        /// </summary>
        public bool UnlockNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            var pathResult = node.AlgorithmService.FindPath(node, _skillTreeEntity);
            var canUnlock = _unlockUsecase.Unlock(pathResult.TotalCost.Cost);
            if (!canUnlock) return false;
            foreach (var pathNode in pathResult.Path)
            {
                var skillNode = _skillTreeRepository.GetNode(pathNode.SkillNodeIdVO.Id);
                skillNode.Unlock();
                _skillNodePresenter.Unlock(skillNode.SkillNodeIdVO.Id, skillNode.IsUnlocked);
            }
            return true;
        }
        private readonly UnlockUsecase _unlockUsecase;
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly SkillTreeEntity _skillTreeEntity;
        private readonly SkillNodePresenter _skillNodePresenter;
    }
}
