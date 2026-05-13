namespace DevelopProducts.SkillTree
{
    public class SkillCanUnlockService
    {
        public bool CanUnlock(
            SkillNodeEntity node,
            SkillTreeEntity tree,
            int currentPoints)
        {
            if (node == null) return false;
            if (node.IsUnlocked) return false;
            return currentPoints >= node.UnlockCost.Cost;
        }
    }
}
