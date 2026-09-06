using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     ポインタークリック以外の共通経路からUI要素の作動を要求するイベントです。
    /// </summary>
    public sealed class UIActivationEvent : EventBase<UIActivationEvent>
    {
        /// <summary>
        ///     イベントを伝播可能な初期状態へ戻します。
        /// </summary>
        protected override void Init()
        {
            base.Init();
            bubbles = true;
            tricklesDown = true;
        }
    }
}
