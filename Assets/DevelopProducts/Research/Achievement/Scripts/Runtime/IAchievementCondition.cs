namespace DevelopProducts.Achievement
{
    public interface IAchievementCondition
    {
        public bool CheckAchievement(in AchievementContext context);
    }
}
