using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public ref struct CanUnlockDTO
    {
        public CanUnlockDTO(bool canUnlock)
        {
            CanUnlock = canUnlock;
        }
        public bool CanUnlock { get;}
    }
}
