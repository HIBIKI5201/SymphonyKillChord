using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     インゲーム HUD の ViewModel。
    /// </summary>
    public interface IIngameHudViewModel
    {
        /// <summary>
        ///     HP の表示を更新する。
        /// </summary>
        public void UpdateHealth(in IngameHudDTO dto);
    }
}