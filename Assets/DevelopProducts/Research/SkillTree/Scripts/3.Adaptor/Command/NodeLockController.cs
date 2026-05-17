namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードをロックするコントローラー
    /// </summary>
    public class NodeLockController
    {
        public NodeLockController(ISkillTreeRepository skillTreeRepository,
                                    SkillNodePresenter skillNodePresenter)
        {
            _skillTreeRepository = skillTreeRepository;
            _skillNodePresenter = skillNodePresenter;
        }

        public void LockNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            node.Lock();
            _skillNodePresenter.Visible(nodeId, node.IsEnable);
        }
        private readonly ISkillTreeRepository _skillTreeRepository;
        private readonly SkillNodePresenter _skillNodePresenter;
    }
}
