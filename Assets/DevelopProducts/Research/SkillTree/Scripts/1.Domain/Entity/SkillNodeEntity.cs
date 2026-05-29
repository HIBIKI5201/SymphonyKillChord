namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードのEntity
    /// </summary>
    public class SkillNodeEntity
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="cost"></param>
        /// <param name="algorithmService"></param>
        /// <param name="isUnlocked"></param>
        /// <param name="isEnable"></param>
        /// <param name="isOrigin"></param>
        public SkillNodeEntity(int nodeId,
            int cost,
            IAlgorithmService algorithmService)
        {
            SkillNodeIdVO = new SkillNodeId(nodeId);
            UnlockCost = new UnlockCost(cost);
            AlgorithmService = algorithmService;
        }
        /// <summary>ノードのID</summary>
        public SkillNodeId SkillNodeIdVO { get; }
        /// <summary>解放に必要なコスト</summary>
        public UnlockCost UnlockCost { get; }
        /// <summary>解放するアルゴリズムサービス</summary>
        public IAlgorithmService AlgorithmService { get; }
        /// <summary>解放されているかどうかの変数</summary>
        public bool IsUnlocked => _isUnlocked;
        /// <summary>原点かどうかの変数</summary>
        public bool IsOrigin => Parents.Length == 0;
        /// <summary>表示されているかどうかの変数</summary>
        public bool IsEnable => _isEnable;
        /// <summary>親ノード</summary>
        public SkillNodeEntity[] Parents => _parents;
        /// <summary>
        ///     親を初期化
        ///     Node全体のInitializeを終わらせてから呼び出す
        /// </summary>
        /// <param name="parents"></param>
        public void SetParent(SkillNodeEntity[] parents)
        {
            _parents = parents;
        }
        /// <summary>
        ///     ノードの解放
        /// </summary>
        public void Unlock()
        {
            _isUnlocked = true;
        }
        /// <summary>
        ///     ノードを表示させる
        /// </summary>
        public void NodeEnable()
        {
            _isEnable = true;
        }
        /// <summary>
        ///     ノードを非表示にする
        /// </summary>
        public void NodeDisable()
        {
            _isEnable = false;
        }
        private bool _isUnlocked;
        private bool _isEnable;
        private SkillNodeEntity[] _parents;
    }
}