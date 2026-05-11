namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     1ノードが現在解放可能かを判定するサービス
    /// </summary>
    public class SkillUnlockService
    {
        /// <summary>
        ///     解放条件：
        ///       1. 未解放であること
        ///       2. 所持ポイントがコスト以上であること
        /// </summary>
        public bool CanUnlock(
            SkillNodeEntity node,
            SkillTreeEntity tree,
            UnlockCost currentPoints)
        {
            if (node == null) return false;
            if (node.IsUnlocked) return false;
            if (!node.IsEnable) return false;
            if (!tree.IsReachable(node.SkillNodeIdVO)) return false;
            return currentPoints.Cost >= node.UnlockCost.Cost;
        }
    }
}
