namespace DevelopProducts.SkillTree
{
    public interface INodeVM
    {
        event System.Action<bool> CanUnlock;
        event System.Action<bool> Unlocked;
        event System.Action<bool> Visible;
        void CheckUnlock(in CanUnlockDTO dto);
        void Unlock(in UnlockDTO dto);
        void CheckVisible(in CheckVisibleDTO dto);
    }
}
