namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     報酬がパラメーター強化のインターフェース
    /// </summary>
    public interface IParameterUpgradeEffect : INodeUnlockEffect
    {
        new string Description { get; }
    }
}
