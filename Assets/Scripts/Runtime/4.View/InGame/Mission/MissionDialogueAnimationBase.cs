using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     会話UI演出基底クラス。
    /// </summary>
    public abstract class MissionDialogueAnimationBase : MonoBehaviour
    {
        /// <summary>
        ///     直ちに非表示する。
        /// </summary>
        public abstract void HideImmediate();
        /// <summary>
        ///     UI入場を行う。
        /// </summary>
        public abstract void Show();
        /// <summary>
        ///     UI退場を行う。
        /// </summary>
        public abstract void Hide(Action onCompleted);
        /// <summary>
        ///     一時停止を行う。
        /// </summary>
        public abstract void SetPaused(bool isPaused);
    }
}
