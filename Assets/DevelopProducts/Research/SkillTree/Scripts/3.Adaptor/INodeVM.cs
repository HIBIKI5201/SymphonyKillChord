namespace DevelopProducts.SkillTree
{
    public interface INodeVM
    {
        event System.Action<bool> CanUnlock;
        event System.Action<bool> Unlocked;
        event System.Action<bool> Locked;
        void Check(in CanUnlockDTO dto);
        void Unlock(in UnlockDTO dto);
        void Lock(in LockDTO dto);
    }
}
