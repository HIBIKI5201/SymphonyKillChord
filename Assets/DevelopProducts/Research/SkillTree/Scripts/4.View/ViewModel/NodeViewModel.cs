using System;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///    　ノードのViewModel
    /// </summary>
    public class NodeViewModel : INodeVM
    {
        public event Action<bool> CanUnlock;
        public event Action<bool> Unlocked;
        public event Action<bool> Visible;
        /// <summary>
        ///     ノードが解放可能かのイベントを発火させる
        /// </summary>
        /// <param name="dto"></param>
        public void CheckUnlock(in CanUnlockDTO dto)
        {
            CanUnlock?.Invoke(dto.CanUnlock);
        }
        /// <summary>
        ///     ノードが可視化可能かのイベントを発火させる
        /// </summary>
        /// <param name="dto"></param>
        public void CheckVisible(in CheckVisibleDTO dto)
        {
            Visible?.Invoke(dto.isVisible);
        }
        /// <summary>
        ///     ノード解放のイベントを発火させる
        /// </summary>
        /// <param name="dto"></param>
        public void Unlock(in UnlockDTO dto)
        {
            Unlocked?.Invoke(dto.IsUnlock);
        }
    }
}
