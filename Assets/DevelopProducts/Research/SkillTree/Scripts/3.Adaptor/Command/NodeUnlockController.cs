namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///  ノードを解放するコントローラー
    /// </summary>
    public class NodeUnlockController
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="unlockUsecase">スキルをアンロックするユースケース</param>
        public NodeUnlockController(UnlockUsecase unlockUsecase,
                                    ISkillTreeRepository skillTreeRepository,
                                    SkillNodePresenter skillNodePresenter)
        {
            _unlockUsecase = unlockUsecase;
            _skillTreeRepository = skillTreeRepository;
            _skillNodePresenter = skillNodePresenter;
        }
        /// <summary>
        ///     ノードを解放する
        /// </summary>
        public bool UnlockNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            var pathResult = node.AlgorithmService.FindPath(node, _skillTreeRepository);
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
        private readonly SkillNodePresenter _skillNodePresenter;
    }
}
