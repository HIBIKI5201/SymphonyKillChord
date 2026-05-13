namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     報酬がスキルのノードエフェクトインターフェース
    /// </summary>
    public interface ISkillUnlockEffect : INodeUnlockEffect
    {
        new string Description { get; }
    }
}
