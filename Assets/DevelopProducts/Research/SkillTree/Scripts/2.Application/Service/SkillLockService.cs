namespace DevelopProducts.SkillTree
{
    public class SkillLockService
    {
        public void LockNode(SkillNodeEntity node)
        {
            node.Lock();
        }
    }
}
