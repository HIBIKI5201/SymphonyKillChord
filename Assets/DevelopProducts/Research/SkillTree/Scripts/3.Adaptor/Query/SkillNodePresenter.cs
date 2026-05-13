namespace DevelopProducts.SkillTree
{
    public class SkillNodePresenter
    {
        public SkillNodePresenter(ISkillTreeRepository skillTreeRepository)
        {
            _skillTreeRepository = skillTreeRepository;
        }
        public void Unlock(int nodeId)
        {
            _skillTreeRepository.GetNode(nodeId).Unlock();
        }
        private readonly ISkillTreeRepository _skillTreeRepository;
    }
}
