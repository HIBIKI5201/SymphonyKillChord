namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     NodeEntityが解放されているかどうかを渡すDTO
    /// </summary>
    public ref struct UnlockDTO
    {
        public UnlockDTO(bool isUnlock)
        {
            IsUnlock = isUnlock;
        }
        public bool IsUnlock { get; }
    }
}
