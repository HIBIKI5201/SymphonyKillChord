using UnityEngine;

namespace DevelopProducts.Achievement
{
    public class AchievementElement
    {
        public AchievementElement(
            AchievementElementID id,
            AchievementElementID[] dependences,
            IAchievementCondition[] conditions)
        {
            _id = id;
            _dependences = dependences;
            _conditions = conditions;
        }

        public bool Check(in AchievementContext context)
        {
            for (int i = 0; i < _conditions.Length; i++)
            {
                bool result = _conditions[i].CheckAchievement(context);
                if (!result) { return false; }
            }

            return true;
        }

        private readonly AchievementElementID _id;
        private readonly AchievementElementID[] _dependences;
        private readonly IAchievementCondition[] _conditions;
    }
}
