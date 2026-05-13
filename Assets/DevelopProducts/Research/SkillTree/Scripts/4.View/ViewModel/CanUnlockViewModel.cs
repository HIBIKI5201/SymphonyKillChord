using System;

namespace DevelopProducts.SkillTree
{
    public class CanUnlockViewModel : ICanUnlockVM
    {
        public event Action<bool> CanUnlock;
        public void Push(in CanUnlockDTO dto)
        {
            CanUnlock?.Invoke(dto.CanUnlock);
        }
    }
}
