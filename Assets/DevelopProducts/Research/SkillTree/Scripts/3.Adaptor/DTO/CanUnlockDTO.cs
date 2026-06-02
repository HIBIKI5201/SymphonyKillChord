namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     NodeEntityが解放できるかどうかを渡すDTO
    /// </summary>
    public ref struct CanUnlockDTO
    {
        public CanUnlockDTO(bool canUnlock)
        {
            CanUnlock = canUnlock;
        }
        public bool CanUnlock { get;}
    }
}
