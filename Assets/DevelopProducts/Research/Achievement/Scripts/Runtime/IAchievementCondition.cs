namespace DevelopProducts.Achievement
{
    public interface IAchievementCondition
    {
        public bool CheckAchievement(AchievementContext context);
    }
}
