namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     ノードが可視化されているかどうか渡すDTO
    /// </summary>
    public ref struct CheckVisibleDTO
    {
        public CheckVisibleDTO(bool isVisible)
        {
            this.isVisible = isVisible;
        }
        public bool isVisible { get; }
    }
}
