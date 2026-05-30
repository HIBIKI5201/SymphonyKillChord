namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードが解放可能か調べるコントローラ
    /// </summary>
    public class NodeCanUnlockController
    {
        public NodeCanUnlockController(ISkillTreeRepository skillTreeRepository,
                                       CanUnlockUsecase unlockUsecase,
                                       SkillNodePresenter presenter)
        {
            _skillTreeRepository = skillTreeRepository;
            _unlockUsecase = unlockUsecase;
            _skillsNodePresenter = presenter;
        }
        /// <summary>
        ///     ノードが解放できるか調べる
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool CanUnlock(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            var path = node.AlgorithmService.FindPath(node, _skillTreeRepository);
            var canUnlock = _unlockUsecase.CheckUnlock(path.TotalCost.Cost);

            _skillsNodePresenter.CanUnlock(nodeId, canUnlock);
            return canUnlock;
        }
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly CanUnlockUsecase _unlockUsecase;
        private readonly SkillNodePresenter _skillsNodePresenter;
    }
}
