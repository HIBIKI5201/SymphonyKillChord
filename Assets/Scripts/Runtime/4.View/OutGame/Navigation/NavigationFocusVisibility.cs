using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     フォーカスを表示してよい入力状態かを保持し、ポインター操作中はフォーカスを外します。
    ///     <para>
    ///         マウス・タッチ操作中に初期フォーカスやクリックによるフォーカスが残ると、
    ///         :hover と :focus の装飾が二重に付くため、ナビゲーション可能な要素のフォーカスを外して記憶しておきます。
    ///         コントローラー・キーボードの入力に切り替わったときに、記憶した要素からフォーカスを再開します。
    ///     </para>
    ///     <para>
    ///         <see cref="UINavigationExtensions.MakeNavigable"/> と <see cref="UINavigationExtensions.FocusDeferred"/>
    ///         という静的な拡張メソッドから参照するため、状態は静的に持ちます。
    ///     </para>
    /// </summary>
    public static class NavigationFocusVisibility
    {
        /// <summary> コントローラー・キーボードによるナビゲーション操作中かです。falseの間はフォーカスを表示しません。 </summary>
        public static bool IsNavigationActive => _isNavigationActive;

        /// <summary>
        ///     ナビゲーション操作中かを切り替えます。
        ///     ポインター操作へ切り替わったときは現在のフォーカスを外し、
        ///     ナビゲーション操作へ切り替わったときは記憶した要素へフォーカスを戻します。
        /// </summary>
        /// <param name="isNavigationActive"> ナビゲーション操作中であればtrueです。 </param>
        public static void SetNavigationActive(bool isNavigationActive)
        {
            if (_isNavigationActive == isNavigationActive)
            {
                return;
            }

            _isNavigationActive = isNavigationActive;
            NavigationDebugLog.Log($"Navigation focus visibility changed: {isNavigationActive}");

            if (isNavigationActive)
            {
                RestoreFocus();
            }
            else
            {
                SuppressFocus(_lastFocused?.focusController?.focusedElement as VisualElement);
            }
        }

        /// <summary>
        ///     フォーカスを得た要素を記憶し、ポインター操作中であればフォーカスを外します。
        /// </summary>
        /// <param name="element"> フォーカスを得た要素です。 </param>
        internal static void HandleFocused(VisualElement element)
        {
            _lastFocused = element;
            if (!_isNavigationActive)
            {
                // フォーカス処理の最中に Blur するとフォーカス制御の状態と食い違うため、次の更新で外す。
                element.schedule.Execute(() => BlurIfPointerMode(element));
            }
        }

        /// <summary>
        ///     ポインター操作中に要求されたフォーカス先を、フォーカスさせずに記憶します。
        /// </summary>
        /// <param name="element"> フォーカス先の要素です。 </param>
        internal static void RememberPending(VisualElement element)
        {
            _lastFocused = element;
        }

        private static bool _isNavigationActive = true;
        private static VisualElement _lastFocused;

        /// <summary>
        ///     ポインター操作中のままであれば、要素のフォーカスを外します。
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        private static void BlurIfPointerMode(VisualElement element)
        {
            if (_isNavigationActive || element.panel == null)
            {
                return;
            }

            if (element.focusController?.focusedElement == element)
            {
                element.Blur();
            }
        }

        /// <summary>
        ///     ナビゲーション可能な要素がフォーカス中であれば記憶して外します。
        /// </summary>
        /// <param name="focused"> 現在フォーカス中の要素です。 </param>
        private static void SuppressFocus(VisualElement focused)
        {
            if (focused == null || !focused.ClassListContains(UINavigationExtensions.NAVIGABLE_CLASS_NAME))
            {
                return;
            }

            _lastFocused = focused;
            focused.Blur();
        }

        /// <summary>
        ///     記憶した要素がまだ表示されていて、他にフォーカス中の要素が無ければフォーカスを戻します。
        /// </summary>
        private static void RestoreFocus()
        {
            VisualElement target = _lastFocused;
            if (target == null || target.panel == null || !target.focusable)
            {
                return;
            }

            if (target.focusController?.focusedElement != null)
            {
                return;
            }

            target.FocusDeferred();
        }
    }
}
