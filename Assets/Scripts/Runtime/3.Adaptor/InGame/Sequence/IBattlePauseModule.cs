using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     戦闘ポーズモジュールのインタフェース。
    /// </summary>
    public interface IBattlePauseModule
    {
        /// <summary>
        ///     戦闘ポーズを実行する。
        /// </summary>
        /// <returns> ポーズできた場合は true </returns>
        public bool TryPause();

        /// <summary>
        ///     戦闘ポーズを解除する。
        /// </summary>
        public void Resume();
    }
}
