using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///   ゲームプレイの開始と停止の対象に機能の切り替えを行うためのインターフェース。
    /// </summary>
    public interface IGameplayControllable
    {
        /// <summary>
        ///   ゲームプレイの開始処理を行います。
        /// </summary>
        void StartGameplay();

        /// <summary>
        ///   ゲームプレイの停止処理を行います。
        /// </summary>
        void StopGameplay();
    }
}
