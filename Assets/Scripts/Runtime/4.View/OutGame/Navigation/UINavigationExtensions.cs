using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     UI Toolkit の要素をマウスとコントローラーの両方で操作できるようにする拡張メソッド群です。
    ///     <para>
    ///         UI Toolkit はフォーカスを持つ要素にしか <see cref="NavigationSubmitEvent"/> を配送しないため、
    ///         コントローラー対応には要素を focusable にしたうえで Submit を購読する必要があります。
    ///         クリック経路はそのまま残すため、マウス操作の挙動は変わりません。
    ///     </para>
    /// </summary>
    public static class UINavigationExtensions
    {
        /// <summary> フォーカス可能にした要素へ付与するUSSクラスです。フォーカスリングの装飾に使用します。 </summary>
        public const string NAVIGABLE_CLASS_NAME = "navigable";

        /// <summary>
        ///     画面を開いたときに最初にフォーカスさせる要素へ付与するUSSクラスです。
        ///     <para>
        ///         ノードのように実行時に生成される要素では、生成側がこのクラスを付け、
        ///         画面側はクラス名で引くことで互いに直接参照せずに済みます。
        ///     </para>
        /// </summary>
        public const string INITIAL_FOCUS_CLASS_NAME = "initial-focus";

        /// <summary>
        ///     要素の作動要求を1つの処理へ接続します。
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <param name="onActivate"> 作動時に呼び出す処理です。 </param>
        /// <returns> 登録したコールバックを解除するオブジェクトです。 </returns>
        /// <exception cref="ArgumentNullException"> いずれかがnullの場合にスローされます。 </exception>
        public static IDisposable RegisterActivation(this VisualElement element, Action onActivate)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (onActivate == null)
            {
                throw new ArgumentNullException(nameof(onActivate));
            }

            return new ActivationRegistration(element, onActivate);
        }

        /// <summary>
        ///     要素をコントローラーのフォーカス移動の対象にします。
        ///     <para>
        ///         既にフォーカス可能な要素(Buttonなど)にも呼び出せます。USSクラスの付与のみ行います。
        ///     </para>
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <returns> 呼び出しを連結できるよう、対象の要素をそのまま返します。 </returns>
        /// <exception cref="ArgumentNullException"> elementがnullの場合にスローされます。 </exception>
        public static VisualElement MakeNavigable(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.focusable = true;

            // tabIndex が負の要素はフォーカス移動の探索対象から外れる。
            if (element.tabIndex < 0)
            {
                element.tabIndex = 0;
            }

            // フォーカス中の要素はクリックを透過させないようにしておく。
            if (element.pickingMode == PickingMode.Ignore)
            {
                element.pickingMode = PickingMode.Position;
            }

            element.AddToClassList(NAVIGABLE_CLASS_NAME);
            return element;
        }

        /// <summary>
        ///     要素をコントローラーのフォーカス移動の対象から外します。
        ///     <para>
        ///         キャンセル操作で代替できる「戻る」ボタンなど、
        ///         フォーカスを当てる必要のない要素に使用します。
        ///         マウスでのクリックは従来どおり可能です。
        ///     </para>
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <returns> 呼び出しを連結できるよう、対象の要素をそのまま返します。 </returns>
        /// <exception cref="ArgumentNullException"> elementがnullの場合にスローされます。 </exception>
        public static VisualElement ExcludeFromNavigation(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            // Button など既定でフォーカス可能な要素があるため、明示的に無効化する。
            element.focusable = false;
            element.tabIndex = -1;
            element.RemoveFromClassList(NAVIGABLE_CLASS_NAME);
            return element;
        }

        /// <summary>
        ///     レイアウト確定後に要素へフォーカスを移します。
        ///     <para>
        ///         表示直後の要素は同フレームではまだレイアウトされておらず、
        ///         その場で <see cref="Focus"/> を呼んでも無視されることがあります。
        ///         次のレイアウト後まで遅延させることで確実にフォーカスさせます。
        ///     </para>
        /// </summary>
        /// <param name="element"> フォーカスさせる要素です。nullの場合は何もしません。 </param>
        public static void FocusDeferred(this VisualElement element)
        {
            if (element == null || element.panel == null)
            {
                return;
            }

            element.schedule.Execute(() =>
            {
                // 遅延実行までの間に画面が閉じられている場合があるため、都度確認する。
                if (element.panel == null || !element.enabledInHierarchy)
                {
                    return;
                }

                if (element.resolvedStyle.display == DisplayStyle.None)
                {
                    return;
                }

                element.Focus();
            });
        }

        /// <summary>
        ///     要素の作動コールバックを所有し、一括解除します。
        /// </summary>
        private sealed class ActivationRegistration : IDisposable
        {
            /// <summary>
            ///     作動コールバックを登録します。
            /// </summary>
            /// <param name="element"> 登録対象の要素です。 </param>
            /// <param name="onActivate"> 作動時に呼び出す処理です。 </param>
            public ActivationRegistration(VisualElement element, Action onActivate)
            {
                _element = element;
                _onActivate = onActivate;
                _clickCallback = HandleClickHandler;
                _submitCallback = HandleSubmitHandler;
                _activationCallback = HandleActivationHandler;

                _element.RegisterCallback(_clickCallback);
                _element.RegisterCallback(_submitCallback);
                _element.RegisterCallback(_activationCallback);
            }

            /// <summary>
            ///     登録した全コールバックを解除します。
            /// </summary>
            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _element.UnregisterCallback(_clickCallback);
                _element.UnregisterCallback(_submitCallback);
                _element.UnregisterCallback(_activationCallback);
                _isDisposed = true;
            }

            private readonly VisualElement _element;
            private readonly Action _onActivate;
            private readonly EventCallback<ClickEvent> _clickCallback;
            private readonly EventCallback<NavigationSubmitEvent> _submitCallback;
            private readonly EventCallback<UIActivationEvent> _activationCallback;
            private bool _isDisposed;

            /// <summary>
            ///     ポインタークリックによる作動要求を処理します。
            /// </summary>
            /// <param name="clickEvent"> クリックイベントです。 </param>
            private void HandleClickHandler(ClickEvent clickEvent)
            {
                Activate(clickEvent);
            }

            /// <summary>
            ///     ナビゲーション決定による作動要求を処理します。
            /// </summary>
            /// <param name="submitEvent"> ナビゲーション決定イベントです。 </param>
            private void HandleSubmitHandler(NavigationSubmitEvent submitEvent)
            {
                Activate(submitEvent);
            }

            /// <summary>
            ///     明示的な作動要求を処理します。
            /// </summary>
            /// <param name="activationEvent"> 明示的な作動イベントです。 </param>
            private void HandleActivationHandler(UIActivationEvent activationEvent)
            {
                Activate(activationEvent);
            }

            /// <summary>
            ///     有効な要素の作動処理を1回実行します。
            /// </summary>
            /// <param name="activationEvent"> 処理対象のイベントです。 </param>
            private void Activate(EventBase activationEvent)
            {
                if (_isDisposed || !_element.enabledInHierarchy)
                {
                    return;
                }

                _onActivate();
                activationEvent.StopPropagation();
            }
        }
    }
}
