using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンの画面 ID と View の対応表クラス。
    /// </summary>
    public class TitleScreenViewRegistry : IScreenViewRegistry, IDisposable
    {
        /// <summary>
        ///    Registry を初期化します。
        /// </summary>
        /// <param name="titleScreenView"></param>
        /// <param name="menuScreenView"></param>
        /// <param name="optionsScreenView"></param>
        /// <param name="creditScreenView"></param>
        public TitleScreenViewRegistry(
            ScreenViewBase titleScreenView,
            ScreenViewBase menuScreenView,
            ScreenViewBase optionsScreenView,
            ScreenViewBase creditScreenView)
        {
            _views = new Dictionary<ScreenId, ScreenViewBase>
            {
                { ScreenId.Title, titleScreenView },
                { ScreenId.Menu, menuScreenView },
                { ScreenId.Options, optionsScreenView },
                { ScreenId.Credit, creditScreenView },
            };
        }

        /// <summary>
        ///    指定された画面 ID の View を表示状態にします。
        /// </summary>
        /// <param name="screenId"></param>
        public void Show(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out ScreenViewBase view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }

            if (_focusToRestore != null)
            {
                view.SetInitialFocusElement(_focusToRestore);
                _focusToRestore = null;
            }
            else if (_currentScreenId.HasValue &&
                _currentScreenId.Value != screenId &&
                _views.TryGetValue(_currentScreenId.Value, out ScreenViewBase currentView))
            {
                VisualElement focusedElement = currentView.FocusedElement;
                if (focusedElement != null && focusedElement.panel != null)
                {
                    _focusHistory.Push(focusedElement);
                }
            }

            _currentScreenId = screenId;
            view.Show();
        }

        /// <summary>
        ///     指定された画面 ID の View を非表示状態にします。
        /// </summary>
        /// <param name="screenId"></param>
        public void Hide(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out ScreenViewBase view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }

            view.Hide();

            if (_currentScreenId != screenId)
            {
                return;
            }

            _currentScreenId = null;
            _focusToRestore = PopAvailableFocusElement();
        }

        /// <summary>
        ///     フォーカス履歴を破棄し、次の画面を既定のフォーカス位置から表示する。
        /// </summary>
        public void ResetFocusHistory()
        {
            _focusHistory.Clear();
            _focusToRestore = null;
            _currentScreenId = null;
        }

        /// <summary>
        ///   すべての View を非表示状態にします。
        /// </summary>
        public void HideAll()
        {
            foreach (IScreenView view in _views.Values)
            {
                view.Hide();
            }
        }

        /// <summary>
        ///     すべての View をフェードなしで即座に非表示状態にします。
        /// </summary>
        public void HideAllImmediately()
        {
            foreach (ScreenViewBase view in _views.Values)
            {
                view.HideImmediately();
            }
        }

        /// <summary>
        ///     レジストリに登録されている全ての画面のリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            foreach (IDisposable disposable in _views.Values)
            {
                disposable?.Dispose();
            }

            ResetFocusHistory();
        }

        /// <summary>
        ///     登録画面の入力許可を切り替え、再開時は現在画面だけへフォーカス復元を要求する。
        /// </summary>
        public void SetInteractionEnabled(bool isEnabled)
        {
            if (_isDisposed || _isInteractionEnabled == isEnabled)
            {
                return;
            }

            _isInteractionEnabled = isEnabled;
            foreach (ScreenViewBase view in _views.Values)
            {
                view.SetInteractionEnabled(isEnabled);
            }

            if (isEnabled && _currentScreenId.HasValue
                && _views.TryGetValue(_currentScreenId.Value, out ScreenViewBase currentView))
            {
                currentView.RestoreFocus();
            }
        }

        private bool _isInteractionEnabled = true;
        private bool _isDisposed;
        private readonly Dictionary<ScreenId, ScreenViewBase> _views;
        private readonly Stack<VisualElement> _focusHistory = new();
        private ScreenId? _currentScreenId;
        private VisualElement _focusToRestore;

        /// <summary>
        ///     履歴から、現在もパネルに存在するフォーカス先を取り出す。
        /// </summary>
        /// <returns> 復帰可能な要素。存在しない場合はnull。 </returns>
        private VisualElement PopAvailableFocusElement()
        {
            while (_focusHistory.Count > 0)
            {
                VisualElement focusElement = _focusHistory.Pop();
                if (focusElement != null && focusElement.panel != null)
                {
                    return focusElement;
                }
            }

            return null;
        }
    }
}
