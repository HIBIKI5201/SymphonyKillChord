namespace DevelopProducts.SkillTree
{
    public ref struct LockDTO
    {
        public LockDTO(bool isLock)
        {
            IsLock = isLock;
        }
        public bool IsLock { get; }
    }
}
