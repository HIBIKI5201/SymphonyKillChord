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
        /// <param name="skillUnlockService">スキルをアンロックするサービス</param>
        public NodeUnlockController(SkillUnlockService skillUnlockService)
        {
            _skillUnlockService = skillUnlockService;
        }
        /// <summary>
        /// ノードを
        /// </summary>
        public void UnlockNode()
        {

        }
        private readonly SkillUnlockService _skillUnlockService;
    }
}
