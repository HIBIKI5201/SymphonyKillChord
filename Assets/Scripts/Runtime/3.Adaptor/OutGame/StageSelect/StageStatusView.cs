using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     View 層向けのステージの状態を表す列挙型。
    /// </summary>
    public enum StageStatusView
    {
        /// <summary> ステージがロックされている状態。 </summary>
        Locked,
        /// <summary> ステージが解放されている状態。 </summary>
        Unlocked,
        /// <summary> ステージがクリアされている状態。 </summary>
        Cleared,
    }
}
