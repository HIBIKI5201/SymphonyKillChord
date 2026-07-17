namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードの表示を切り替えるコントローラ
    /// </summary>
    public class NodeVisibleController
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="skillTreeRepository"></param>
        /// <param name="skillNodePresenter"></param>
        public NodeVisibleController(SkillTreeRepository skillTreeRepository, SkillNodePresenter skillNodePresenter)
        {
            _skillTreeRepository = skillTreeRepository;
            _skillNodePresenter = skillNodePresenter;
        }
        /// <summary>
        ///     ノードを表示させる
        /// </summary>
        /// <param name="nodeId"></param>
        public void VisibleNode(int nodeId)
        {
            var node = _skillTreeRepository.GetNode(nodeId);
            node.NodeEnable();
            _skillNodePresenter.Visible(nodeId, node.IsEnable);
        }
        /// <summary>
        ///     ノードを非表示にする
        /// </summary>
        /// <param name="nodeId"></param>
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
