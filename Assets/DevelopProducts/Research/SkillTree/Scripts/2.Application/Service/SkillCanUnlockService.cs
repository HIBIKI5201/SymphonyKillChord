namespace DevelopProducts.SkillTree
{
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
