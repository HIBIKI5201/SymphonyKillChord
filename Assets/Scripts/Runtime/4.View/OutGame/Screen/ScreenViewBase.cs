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
        ///     画面を非表示状態にします。実際の見た目の変化は USS のトランジションに従います。
        /// </summary>
        public virtual void Hide()
        {
            RootElement.AddToClassList(HIDDEN_CLASS);
            RootElement.RemoveFromClassList(VISIBLE_CLASS);

            // display を切ると同フレームでレイアウトから外れ、
            // USS のフェードアウトが描画されないままになる。
            if (!KeepLayoutWhileHidden)
            {
                RootElement.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        ///     非表示中もレイアウトに残すかどうかです。
        ///     true にすると非表示時に display を操作しないため、USS のフェードアウトが再生されます。
        ///     画面全体を覆わない View で true にすると、透明な状態で入力を遮るので注意してください。
        /// </summary>
        protected virtual bool KeepLayoutWhileHidden => false;

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
