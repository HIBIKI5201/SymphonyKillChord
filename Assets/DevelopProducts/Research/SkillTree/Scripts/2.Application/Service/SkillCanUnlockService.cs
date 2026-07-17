namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードが解放可能かを調べるサービス
    /// </summary>
    public class SkillCanUnlockService
    {
        public bool CanUnlock(
            int cost,
            int currentPoints)
        {
            return currentPoints >= cost;
        }
    }
}
