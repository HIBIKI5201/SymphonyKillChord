using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Screen
{
    /// <summary>
    ///     画面 ID と View の対応表クラス。
    /// </summary>
    public sealed class ScreenViewRegistry : IScreenViewRegistry, IDisposable
    {
        /// <summary> Registry を初期化します。 </summary>
        public ScreenViewRegistry(
            ScreenViewBase homeScreenView,
            ScreenViewBase stageSelectScreenView,
            ScreenViewBase skillTreeScreenView,
            ScreenViewBase skillBuildScreenView,
            ScreenViewBase battlePreparationScreenView,
            ScreenViewBase settingScreenView)
        {
            _views = new Dictionary<ScreenId, ScreenViewBase>
            {
                { ScreenId.Home, homeScreenView },
                { ScreenId.StageSelect, stageSelectScreenView },
                { ScreenId.SkillTree, skillTreeScreenView },
                { ScreenId.SkillBuild, skillBuildScreenView },
                { ScreenId.BattlePreparation, battlePreparationScreenView },
                { ScreenId.Setting, settingScreenView },
            };
        }

        /// <summary>
        ///    指定画面を表示状態にします。
        /// </summary>
        /// <param name="screenId"></param>
        public void Show(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out ScreenViewBase view))
            {
                Debug.LogWarning(
                    $"[{nameof(ScreenViewRegistry)}] "
                    + $"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }

            if (TryTakeFocusToRestore(screenId, out VisualElement focusElement))
            {
                view.SetInitialFocusElement(focusElement);
            }
            else
            {
                PushPendingFocus(screenId);
            }

            ClearPendingFocus();
            _currentScreenId = screenId;
            view.Show();
        }

        /// <summary>
        ///    指定画面を非表示状態にします。
        /// </summary>
        public void Hide(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out ScreenViewBase view))
            {
                Debug.LogWarning(
                    $"[{nameof(ScreenViewRegistry)}] "
                    + $"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }

            if (_currentScreenId == screenId)
            {
                _pendingScreenId = screenId;
                _pendingFocusElement = view.FocusedElement;
                _currentScreenId = null;
            }

            view.Hide();
        }

        /// <summary>
        ///     全画面を非表示状態にします。
        /// </summary>
        public void HideAll()
        {
            foreach (IScreenView screenView in _views.Values)
            {
                screenView.Hide();
            }

            ResetFocusHistory();
        }

        /// <summary>
        ///     全画面をフェードなしで即座に非表示状態にします。
        /// </summary>
        public void HideAllImmediately()
        {
            foreach (ScreenViewBase screenView in _views.Values)
            {
                screenView.HideImmediately();
            }

            ResetFocusHistory();
        }

        /// <summary>
        ///     レジストリに登録されている全ての画面のリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            foreach (IDisposable disposable in _views.Values)
            {
                disposable.Dispose();
            }

            ResetFocusHistory();
        }

        private readonly IReadOnlyDictionary<ScreenId, ScreenViewBase> _views;
        private readonly List<(ScreenId ScreenId, VisualElement FocusElement)> _focusHistory = new();
        private ScreenId? _currentScreenId;
        private ScreenId? _pendingScreenId;
        private VisualElement _pendingFocusElement;

        /// <summary>
        ///     表示先が履歴内の画面なら、対応するフォーカス先を取り出す。
        /// </summary>
        /// <param name="screenId"> 表示する画面ID。 </param>
        /// <param name="focusElement"> 復元するフォーカス先。 </param>
        /// <returns> 復元可能なフォーカス先が存在する場合はtrue。 </returns>
        private bool TryTakeFocusToRestore(
            ScreenId screenId,
            out VisualElement focusElement)
        {
            if (_pendingScreenId == screenId
                && IsAvailableFocusElement(_pendingFocusElement))
            {
                focusElement = _pendingFocusElement;
                return true;
            }

            for (int i = _focusHistory.Count - 1; i >= 0; i--)
            {
                (ScreenId historyScreenId, VisualElement historyFocusElement) =
                    _focusHistory[i];

                if (historyScreenId != screenId)
                {
                    continue;
                }

                _focusHistory.RemoveRange(
                    i,
                    _focusHistory.Count - i);

                if (IsAvailableFocusElement(historyFocusElement))
                {
                    focusElement = historyFocusElement;
                    return true;
                }
            }

            focusElement = null;
            return false;
        }

        /// <summary>
        ///     非表示にした画面のフォーカス先を、新しい遷移元として履歴へ積む。
        /// </summary>
        /// <param name="nextScreenId"> 次に表示する画面ID。 </param>
        private void PushPendingFocus(ScreenId nextScreenId)
        {
            if (!_pendingScreenId.HasValue
                || _pendingScreenId.Value == nextScreenId
                || !IsAvailableFocusElement(_pendingFocusElement))
            {
                return;
            }

            _focusHistory.Add(
                (_pendingScreenId.Value, _pendingFocusElement));
        }

        /// <summary>
        ///     フォーカス先が現在もパネルに存在するか判定する。
        /// </summary>
        /// <param name="focusElement"> 判定するフォーカス先。 </param>
        /// <returns> 復元可能な場合はtrue。 </returns>
        private static bool IsAvailableFocusElement(VisualElement focusElement)
        {
            return focusElement != null && focusElement.panel != null;
        }

        /// <summary>
        ///     直前に非表示にした画面の一時フォーカス情報を破棄する。
        /// </summary>
        private void ClearPendingFocus()
        {
            _pendingScreenId = null;
            _pendingFocusElement = null;
        }

        /// <summary>
        ///     画面とフォーカスの履歴を初期状態へ戻す。
        /// </summary>
        private void ResetFocusHistory()
        {
            _focusHistory.Clear();
            _currentScreenId = null;
            ClearPendingFocus();
        }
    }
}
