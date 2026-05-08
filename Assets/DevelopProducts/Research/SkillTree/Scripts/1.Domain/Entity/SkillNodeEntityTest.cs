namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntityTest
    {
        public SkillNodeEntityTest(int nodeId, SkillNodeEntityTest parent = null)
        {
            SkillNodeIdVOTest = new SkillNodeIdVOTest(nodeId);
            Parent = parent;
        }
        public SkillNodeEntityTest Parent { get; }
        public SkillNodeIdVOTest SkillNodeIdVOTest { get; }

    }
}
