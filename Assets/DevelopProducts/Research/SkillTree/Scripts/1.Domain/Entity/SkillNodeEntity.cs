namespace DevelopProducts.SkillTree
{
    public class SkillNodeEntity
    {
        public SkillNodeEntity(int nodeId, SkillNodeEntity parent = null)
        {
            SkillNodeIdVOTest = new SkillNodeIdVOTest(nodeId);
            Parent = parent;
        }
        public SkillNodeEntity Parent { get; }
        public SkillNodeIdVOTest SkillNodeIdVOTest { get; }

    }
}
