namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードの状態をViewに反映させるプレゼンター
    /// </summary>
    public class SkillNodePresenter
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="nodeRegistry"></param>
        /// <param name="skillTreeRepository"></param>
        public SkillNodePresenter(NodeRegistry nodeRegistry,
            ISkillTreeRepository skillTreeRepository)
        {
            _nodeRegistry = nodeRegistry;
            _skillTreeRepository = skillTreeRepository;
        }
        /// <summary>
        ///     ノードがアンロック可能かどうかをチェック
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="canUnlock"></param>
        public void CanUnlock(int nodeId, bool canUnlock)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.CheckUnlock(new CanUnlockDTO(canUnlock));
            }
        }
        /// <summary>
        ///     ノードをアンロック
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="isUnlock"></param>
        public void Unlock(int nodeId, bool isUnlock)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.Unlock(new UnlockDTO(isUnlock));
            }
        }
        /// <summary>
        ///     ノードをロック
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="isVisible"></param>
        public void Visible(int nodeId, bool isVisible)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                canUnlockVM.CheckVisible(new CheckVisibleDTO(isVisible));
            }
        }
        /// <summary>
        ///     計算したノードの合計コストを取得
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public int GetTotalCost(int nodeId)
        {
            if (_nodeRegistry.TryGet(nodeId, out var canUnlockVM))
            {
                var node = _skillTreeRepository.GetNode(nodeId);
                var pathResult = node.AlgorithmService.FindPath(node, _skillTreeRepository);
                return pathResult.TotalCost.Cost;
            }
            return 0;
        }
        private readonly NodeRegistry _nodeRegistry;
        private readonly ISkillTreeRepository _skillTreeRepository;
    }
}
