using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface INodeVM
    {
        event System.Action<bool> CanUnlock;
        event System.Action<bool> Unlocked;
        void Check(in CanUnlockDTO dto);
        void Unlock(in UnlockDTO dto);
    }
}
