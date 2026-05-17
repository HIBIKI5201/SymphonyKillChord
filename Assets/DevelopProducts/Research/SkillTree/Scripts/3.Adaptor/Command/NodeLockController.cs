namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードをロックするコントローラー
    /// </summary>
    public class NodeLockController
    {
        public NodeLockController(LockUsecase lockUsecase,
                                    ISkillTreeRepository skillTreeRepository,
                                    SkillTreeEntity skillTreeEntity,
                                    SkillNodePresenter skillNodePresenter)
        {
            _skillTreeRepository = skillTreeRepository;
            _skillNodePresenter = skillNodePresenter;
        }

        public void LockNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            node.Lock();
            _skillNodePresenter.Lock(nodeId, node.IsUnlocked);
        }
        private readonly LockUsecase _lockUsecase;
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly SkillTreeEntity _skillTreeEntity;
        private readonly SkillNodePresenter _skillNodePresenter;
    }
}
