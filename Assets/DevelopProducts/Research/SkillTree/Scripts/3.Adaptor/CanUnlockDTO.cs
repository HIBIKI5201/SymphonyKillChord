using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public ref struct CanUnlockDTO
    {
        public CanUnlockDTO(bool canUnlock, int cost)
        {
            CanUnlock = canUnlock;
        }
        public bool CanUnlock { get;}
    }
}
