using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     UI Toolkit 用画面 View の規定クラス。
    /// </summary>
    public abstract class ScreenViewBase : IScreenView, IDisposable
    {
        /// <summary>
        ///     画面 View を初期化します。
        /// </summary>
        public ScreenViewBase(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
        {
            RootElement = rootElement;
            OutGameUIEvent = outGameUIEvent;
        }

        /// <summary>
        ///     画面を表示します。
        /// </summary>
        public virtual void Show()
        {
            RootElement.style.display = DisplayStyle.Flex;
            RootElement.AddToClassList(VISIBLE_CLASS);
            RootElement.RemoveFromClassList(HIDDEN_CLASS);
            RootElement.BringToFront();
        }

        /// <summary>
        ///     画面を非表示にします。
        /// </summary>
        public virtual void Hide()
        {
            RootElement.AddToClassList(HIDDEN_CLASS);
            RootElement.RemoveFromClassList(VISIBLE_CLASS);
            RootElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     即座に画面を非表示にします。
        /// </summary>
        public virtual void HideImmediately()
        {
            RootElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public virtual void Dispose(){}

        /// <summary> USSの画面表示用クラス名。 </summary>
        protected const string VISIBLE_CLASS = "screen-visible";
        /// <summary> USSの画面非表示用クラス名。 </summary>
        protected const string HIDDEN_CLASS = "screen-hidden";

        /// <summary> VisualElement のルート要素を取得します。 </summary>
        protected VisualElement RootElement { get; }
        /// <summary> OutGameUIEvent を取得します。 </summary>
        protected OutGameUIEvent OutGameUIEvent { get; }
    }
}
