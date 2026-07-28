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
        ///    画面を表示状態にします。実際の見た目の変化は USS のトランジションに従います。
        /// </summary>
        public virtual void Show()
        {
            RootElement.style.display = DisplayStyle.Flex;
            RootElement.AddToClassList(VISIBLE_CLASS);
            RootElement.RemoveFromClassList(HIDDEN_CLASS);
            RootElement.BringToFront();
        }

        /// <summary>
        ///     画面を非表示状態にします。
        /// </summary>
        /// <remarks>
        ///     display を切るため、USS のフェードアウトは再生されません。
        ///     レイアウトに残すとインラインの display が USS の .screen-hidden を上書きし、
        ///     透明なまま画面全体を覆って入力を奪うため、ここでは必ずレイアウトから外します。
        /// </remarks>
        public virtual void Hide()
        {
            RootElement.AddToClassList(HIDDEN_CLASS);
            RootElement.RemoveFromClassList(VISIBLE_CLASS);
            RootElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     リソースを解放します。
        /// </summary>
        public virtual void Dispose() { }

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
