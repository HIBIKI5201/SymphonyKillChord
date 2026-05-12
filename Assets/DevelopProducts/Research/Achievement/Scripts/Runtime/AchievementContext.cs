namespace DevelopProducts.Achievement
{
    public readonly ref struct AchievementContext
    {
        public AchievementContext(
            int enemyKillCount)
        {
            _enemyKillCount = enemyKillCount;
        }

        public int EnemyKillCount => _enemyKillCount;

        private readonly int _enemyKillCount;
    }
}
