using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public ref struct UnlockDTO
    {
        public UnlockDTO(bool isUnlock)
        {
            IsUnlock = isUnlock;
        }
        public bool IsUnlock { get; }
    }
}
