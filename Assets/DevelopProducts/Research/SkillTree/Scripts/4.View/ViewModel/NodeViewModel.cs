using System;

namespace DevelopProducts.SkillTree
{
    public class NodeViewModel : INodeVM
    {
        public event Action<bool> CanUnlock;
        public event Action<bool> Unlocked;
        public event Action<bool> Visible;

        public void CheckUnlock(in CanUnlockDTO dto)
        {
            CanUnlock?.Invoke(dto.CanUnlock);
        }

        public void CheckVisible(in CheckVisibleDTO dto)
        {
            Visible?.Invoke(dto.isVisible);
        }

        public void Unlock(in UnlockDTO dto)
        {
            Unlocked?.Invoke(dto.IsUnlock);
        }
    }
}
