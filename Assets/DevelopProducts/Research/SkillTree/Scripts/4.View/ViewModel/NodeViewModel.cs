using System;

namespace DevelopProducts.SkillTree
{
    public class NodeViewModel : INodeVM
    {
        public event Action<bool> CanUnlock;
        public event Action<bool> Unlocked;

        public void Check(in CanUnlockDTO dto)
        {
            CanUnlock?.Invoke(dto.CanUnlock);
        }

        public void Unlock(in UnlockDTO dto)
        {
            Unlocked?.Invoke(dto.IsUnlock);
        }
    }
}
