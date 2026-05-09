namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     報酬がスキルのノードエフェクトインターフェース
    /// </summary>
    public interface ISkillUnlockEffect : INodeUnlockEffect
    {
        string Description { get; } 
    }
}
