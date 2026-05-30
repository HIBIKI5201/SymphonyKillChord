namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     アルゴリズムサービスのインターフェース
    /// </summary>
    public interface IAlgorithmService
    {
        PathResult FindPath(
            SkillNodeEntity target,
            ISkillTreeRepository skillTreeRepository);
    }
}
