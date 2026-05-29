namespace DevelopProducts.SkillTree
{
    public ref struct CheckVisibleDTO
    {
        public CheckVisibleDTO(bool isVisible)
        {
            this.isVisible = isVisible;
        }
        public bool isVisible { get; }
    }
}
