using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Sequence
{
    /// <summary>
    ///     ポーズモジュールのインタフェース。
    /// </summary>
    public interface IBattlePauseModule
    {
        /// <summary>
        ///     ポーズを実行する。
        /// </summary>
        /// <returns> ポーズできた場合は true </returns>
        public bool TryPause();

        /// <summary>
        ///     ポーズを解除する。
        /// </summary>
        public void Resume();
    }
}
