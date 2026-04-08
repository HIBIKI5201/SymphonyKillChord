using UnityEngine;

namespace DevelopProducts.Achievement
{
    public class EnemyKillCountCondition : IAchievementCondition
    {
        public bool CheckAchievement(AchievementContext context)
        {
            return _value < context.EnemyKillCount;
        }

        [SerializeField, Min(1)]
        private float _value = 1;
    }
}
