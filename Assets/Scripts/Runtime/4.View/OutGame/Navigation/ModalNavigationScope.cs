using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     モーダル表示中、その内側だけにコントローラーのフォーカス移動を閉じ込めるクラスです。
    ///     <para>
    ///         UI Toolkit のフォーカス移動はパネル全体を対象にするため、
    ///         ダイアログを重ねただけでは背面の要素へフォーカスが移動してしまいます。
    ///         有効化している間、モーダルの外にある要素を一時的にフォーカス不可にして防ぎます。
    ///     </para>
    ///     <para>
    ///         マウス操作には影響しません。背面のクリック遮断は、
    ///         従来どおりオーバーレイ要素の pickingMode が担当します。
    ///     </para>
    /// </summary>
    public sealed class ModalNavigationScope
    {
        /// <summary> モーダルが有効かどうかを取得します。 </summary>
        public bool IsActive => _modalRoot != null;

        /// <summary>
        ///     モーダルの内側へフォーカスを閉じ込めます。
        /// </summary>
        /// <param name="modalRoot"> モーダルのルート要素です。 </param>
        public void Activate(VisualElement modalRoot)
        {
            if (modalRoot == null || modalRoot.panel == null || IsActive)
            {
                return;
            }

            _modalRoot = modalRoot;
            _previouslyFocused = modalRoot.panel.focusController?.focusedElement as VisualElement;

            CollectOutsideFocusables(modalRoot.panel.visualTree, modalRoot);

            for (int i = 0; i < _disabledElements.Count; i++)
            {
                _disabledElements[i].focusable = false;
            }

            FindFirstFocusable(modalRoot)?.FocusDeferred();
        }

        /// <summary>
        ///     フォーカスの閉じ込めを解除し、元の状態へ戻します。
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            for (int i = 0; i < _disabledElements.Count; i++)
            {
                _disabledElements[i].focusable = true;
            }

            _disabledElements.Clear();
            _modalRoot = null;

            // モーダルを開く前の位置へフォーカスを戻す。
            if (_previouslyFocused != null && _previouslyFocused.panel != null)
            {
                _previouslyFocused.FocusDeferred();
            }

            _previouslyFocused = null;
        }

        private VisualElement _modalRoot;
        private VisualElement _previouslyFocused;
        private readonly List<VisualElement> _disabledElements = new();

        /// <summary>
        ///     モーダルの外にあるフォーカス可能な要素を集めます。
        /// </summary>
        /// <param name="element"> 探索中の要素です。 </param>
        /// <param name="modalRoot"> 除外するモーダルのルートです。 </param>
        private void CollectOutsideFocusables(VisualElement element, VisualElement modalRoot)
        {
            if (element == null || ReferenceEquals(element, modalRoot))
            {
                return;
            }

            if (element.focusable)
            {
                _disabledElements.Add(element);
            }

            for (int i = 0; i < element.hierarchy.childCount; i++)
            {
                CollectOutsideFocusables(element.hierarchy[i], modalRoot);
            }
        }

        /// <summary>
        ///     モーダル内で最初にフォーカスできる要素を探します。
        /// </summary>
        /// <param name="element"> 探索中の要素です。 </param>
        /// <returns> フォーカス可能な要素です。見つからない場合はnullです。 </returns>
        private static VisualElement FindFirstFocusable(VisualElement element)
        {
            if (element == null || element.resolvedStyle.display == DisplayStyle.None)
            {
                return null;
            }

            if (element.focusable && element.enabledInHierarchy)
            {
                return element;
            }

            for (int i = 0; i < element.hierarchy.childCount; i++)
            {
                VisualElement found = FindFirstFocusable(element.hierarchy[i]);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
