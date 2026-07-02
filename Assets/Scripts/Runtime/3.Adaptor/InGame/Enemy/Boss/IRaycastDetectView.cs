using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     Raycastを行うインタフェース。
    /// </summary>
    public interface IRaycastDetectView
    {
        /// <summary>
        ///     警告ラインのターゲット追従を開始します。
        /// </summary>
        public void StartTrackingWarning();

        /// <summary>
        ///     現在の警告方向を固定し、ラインの長さを維持します。
        /// </summary>
        public void LockWarningDirection();

        /// <summary>
        ///     警告ラインを非表示にし、固定方向を解除します。
        /// </summary>
        public void HideWarning();
    }
}
