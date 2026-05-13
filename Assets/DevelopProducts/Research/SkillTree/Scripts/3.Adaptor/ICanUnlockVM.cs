using UnityEngine;

namespace DevelopProducts.SkillTree
{
    public interface ICanUnlockVM
    {
        event System.Action<bool> CanUnlock;
        void Push(in CanUnlockDTO dto);
    }
}
