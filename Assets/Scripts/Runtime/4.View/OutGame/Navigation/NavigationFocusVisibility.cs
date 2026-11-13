using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    ///         uGUI(シナリオ・ポーズ・リザルトなど)も同じ扱いにします。
    ///         ポインター操作中は EventSystem の選択を外して記憶し、:hover 相当のハイライトと Selected の二重表示を防ぎます。
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

                // 切り替えた入力がそのまま決定として届くと、復元した選択が押されてしまうため、次のフレームで戻す。
                _uguiRestoreFrame = Time.frameCount;
                _isUguiRestorePending = true;
            }
            else
            {
                SuppressFocus(_lastFocused?.focusController?.focusedElement as VisualElement);
                _isUguiRestorePending = false;
                SuppressUguiSelection();
            }
        }

        /// <summary>
        ///     uGUI の選択を入力状態に合わせます。入力の更新ごとに呼び出します。
        ///     <para>
        ///         ポインター操作中は、クリックや画面の初期選択で付いた選択を外して記憶します。
        ///         ナビゲーション操作へ切り替わった次のフレームで、記憶した選択を戻します。
        ///     </para>
        /// </summary>
        public static void UpdateUguiSelection()
        {
            if (!_isNavigationActive)
            {
                SuppressUguiSelection();
                return;
            }

            if (_isUguiRestorePending && Time.frameCount > _uguiRestoreFrame)
            {
                _isUguiRestorePending = false;
                RestoreUguiSelection();
            }
        }

        /// <summary>
        ///     ポインター操作中に uGUI で選択させたい要素を、選択させずに記憶します。
        ///     <para>
        ///         ナビゲーション操作へ切り替わったとき、この要素から選択を再開します。
        ///     </para>
        /// </summary>
        /// <param name="target"> 選択を再開する要素です。 </param>
        public static void RememberUguiSelection(GameObject target)
        {
            _lastUguiSelected = target;
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
        private static GameObject _lastUguiSelected;
        private static bool _isUguiRestorePending;
        private static int _uguiRestoreFrame;

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

        /// <summary>
        ///     ポインター操作中であれば、uGUI の選択を記憶して外します。文字入力欄の選択は残します。
        /// </summary>
        private static void SuppressUguiSelection()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.alreadySelecting)
            {
                return;
            }

            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected == null)
            {
                return;
            }

            // 文字入力欄は選択が外れると入力できなくなるため、対象外にする。
            if (selected.TryGetComponent(out TMP_InputField _) || selected.TryGetComponent(out InputField _))
            {
                return;
            }

            _lastUguiSelected = selected;
            eventSystem.SetSelectedGameObject(null);
        }

        /// <summary>
        ///     記憶した uGUI の要素が操作できる状態で、他に選択中の要素が無ければ選択を戻します。
        /// </summary>
        private static void RestoreUguiSelection()
        {
            EventSystem eventSystem = EventSystem.current;
            GameObject target = _lastUguiSelected;
            if (eventSystem == null || target == null || !target.activeInHierarchy
                || eventSystem.currentSelectedGameObject != null)
            {
                return;
            }

            if (target.TryGetComponent(out Selectable selectable) && !selectable.IsInteractable())
            {
                return;
            }

            eventSystem.SetSelectedGameObject(target);
        }
    }
}
