using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.View.OutGame.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Audio
{
    /// <summary>
    ///     UI Toolkit Eventから再生するUI操作音を解決して再生ポートへ渡す。
    /// </summary>
    public sealed class UIToolkitSoundEffectBinder : IDisposable
    {
        /// <summary>
        ///     rootVisualElementへUI操作音のイベント監視を登録する。
        /// </summary>
        /// <param name="root"> イベントを一括監視するrootVisualElement。 </param>
        /// <param name="config"> UI操作音の解決設定。 </param>
        /// <param name="player"> UI操作音の再生ポート。 </param>
        public UIToolkitSoundEffectBinder(
            VisualElement root,
            UISoundEffectConfig config,
            IPlayableAudioSource player)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            CacheTabHeaders();

            _root.RegisterCallback<ClickEvent>(HandleClickedHandler, TrickleDown.TrickleDown);
            _root.RegisterCallback<PointerDownEvent>(HandlePointerDownHandler, TrickleDown.TrickleDown);
            _root.RegisterCallback<NavigationSubmitEvent>(HandleNavigationSubmitHandler, TrickleDown.TrickleDown);
            _root.RegisterCallback<UIActivationEvent>(HandleActivationHandler, TrickleDown.TrickleDown);
        }

        /// <summary>
        ///     rootVisualElementからUI操作音のイベント監視を解除する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _root.UnregisterCallback<ClickEvent>(HandleClickedHandler, TrickleDown.TrickleDown);
            _root.UnregisterCallback<PointerDownEvent>(HandlePointerDownHandler, TrickleDown.TrickleDown);
            _root.UnregisterCallback<NavigationSubmitEvent>(HandleNavigationSubmitHandler, TrickleDown.TrickleDown);
            _root.UnregisterCallback<UIActivationEvent>(HandleActivationHandler, TrickleDown.TrickleDown);
            _isDisposed = true;
        }

        private const string DRAG_COMPLETED_CLASS_NAME = "drag-just-completed";

        private readonly Dictionary<VisualElement, Tab> _tabByHeader = new();
        private readonly VisualElement _root;
        private readonly UISoundEffectConfig _config;
        private readonly IPlayableAudioSource _player;
        private bool _isDisposed;

        /// <summary>
        ///     ClickEventに対応するActivation Cueを解決して再生する。
        /// </summary>
        /// <param name="evt"> 観測したClickEvent。 </param>
        private void HandleClickedHandler(ClickEvent evt)
        {
            if (!IsPrimaryPointerActivation(evt.pointerType, evt.button, evt.isPrimary)
                || evt.target is not VisualElement target)
            {
                return;
            }

            ProcessActivation(target);
        }

        /// <summary>
        ///     主ポインターのPointerDownEventに対応する作動Cueを解決して再生する。
        /// </summary>
        /// <param name="evt"> 観測したPointerDownEvent。 </param>
        private void HandlePointerDownHandler(PointerDownEvent evt)
        {
            if (!IsPrimaryPointerActivation(evt.pointerType, evt.button, evt.isPrimary)
                || evt.target is not VisualElement target)
            {
                return;
            }

            ProcessPointerDownActivation(target);
        }

        /// <summary>
        ///     NavigationSubmitEventに対応するActivation Cueを解決して再生する。
        /// </summary>
        /// <param name="evt"> 観測したNavigationSubmitEvent。 </param>
        private void HandleNavigationSubmitHandler(NavigationSubmitEvent evt)
        {
            if (evt.target is not VisualElement target)
            {
                return;
            }

            if (ProcessActivation(target))
            {
                return;
            }

            // タイトル開始領域はPointerDown作動として設定されているため、
            // 通常Activationで未解決の場合だけ同じCueを決定操作にも適用する。
            ProcessPointerDownActivation(target);
        }

        /// <summary>
        ///     UIActivationEventに対応するActivation Cueを解決して再生する。
        /// </summary>
        /// <param name="evt"> 観測したUIActivationEvent。 </param>
        private void HandleActivationHandler(UIActivationEvent evt)
        {
            if (evt.target is not VisualElement target)
            {
                return;
            }

            ProcessActivation(target);
        }

        /// <summary>
        ///     指定された要素からActivation Cueを解決して再生する。
        /// </summary>
        /// <param name="target"> Activationイベントのtarget要素。 </param>
        /// <returns> 再生または抑止の判断が完了した場合はtrue。対象を解決できない場合はfalse。 </returns>
        internal bool ProcessActivation(VisualElement target)
        {
            if (_isDisposed || target == null)
            {
                return true;
            }

            Button nearestButton = null;
            Tab nearestTab = null;
            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (element.ClassListContains(DRAG_COMPLETED_CLASS_NAME))
                {
                    return true;
                }

                if (!element.enabledInHierarchy)
                {
                    return true;
                }

                if (_config.TryResolveActivationCue(
                    element,
                    out string cueName,
                    out bool hasMultipleMatches))
                {
                    _player.Play(cueName);
                    return true;
                }

                if (hasMultipleMatches)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIToolkitSoundEffectBinder)}] " +
                        $"要素{element.name}にActivation用の音クラスが複数設定されています。");
                    return true;
                }

                nearestButton ??= element as Button;
                if (nearestTab == null
                    && _tabByHeader.TryGetValue(element, out Tab tab))
                {
                    nearestTab = tab;
                }

                if (ReferenceEquals(element, _root))
                {
                    break;
                }
            }

            if (nearestTab != null && nearestTab.enabledInHierarchy)
            {
                if (_config.TryResolveActivationCue(
                    nearestTab,
                    out string cueName,
                    out bool hasMultipleMatches))
                {
                    _player.Play(cueName);
                    return true;
                }

                if (hasMultipleMatches)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIToolkitSoundEffectBinder)}] " +
                        $"要素{nearestTab.name}にActivation用の音クラスが複数設定されています。");
                    return true;
                }

                _player.Play(_config.DefaultButtonActivationCue);
                return true;
            }

            if (nearestButton != null && nearestButton.enabledInHierarchy)
            {
                _player.Play(_config.DefaultButtonActivationCue);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     指定された要素からPointerDown作動用のCueを解決して再生する。
        /// </summary>
        /// <param name="target"> PointerDownEventのtarget要素。 </param>
        internal void ProcessPointerDownActivation(VisualElement target)
        {
            if (_isDisposed || target == null || IsButtonOrTabHeaderDescendant(target))
            {
                return;
            }

            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (!element.enabledInHierarchy)
                {
                    return;
                }

                if (_config.TryResolvePointerDownActivationCue(
                    element,
                    out string cueName,
                    out bool hasMultipleMatches))
                {
                    _player.Play(cueName);
                    return;
                }

                if (hasMultipleMatches)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIToolkitSoundEffectBinder)}] " +
                        $"要素{element.name}にPointerDown作動用の音クラスが複数設定されています。");
                    return;
                }

                if (ReferenceEquals(element, _root))
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     ポインターイベントが左クリックまたは主タッチか確認する。
        /// </summary>
        /// <param name="pointerType"> ポインターの種類。 </param>
        /// <param name="button"> 操作されたボタン。 </param>
        /// <param name="isPrimary"> 主ポインターの場合はtrue。 </param>
        /// <returns> 左クリックまたは主タッチの場合はtrue。 </returns>
        private static bool IsPrimaryPointerActivation(
            string pointerType,
            int button,
            bool isPrimary)
        {
            return pointerType == UnityEngine.UIElements.PointerType.mouse
                ? button == (int)MouseButton.LeftMouse
                : pointerType == UnityEngine.UIElements.PointerType.touch && isPrimary;
        }

        /// <summary>
        ///     指定要素自身または祖先がButtonかTabヘッダーか確認する。
        /// </summary>
        /// <param name="target"> 確認を開始する要素。 </param>
        /// <returns> ButtonまたはTabヘッダー自身か、その子要素の場合はtrue。 </returns>
        private bool IsButtonOrTabHeaderDescendant(VisualElement target)
        {
            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (element is Button || _tabByHeader.ContainsKey(element))
                {
                    return true;
                }

                if (ReferenceEquals(element, _root))
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        ///     TabView配下のTabヘッダーとTab本体の対応を記録する。
        /// </summary>
        private void CacheTabHeaders()
        {
            List<Tab> tabs = _root.Query<Tab>().ToList();
            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                if (tab?.tabHeader != null)
                {
                    _tabByHeader[tab.tabHeader] = tab;
                }
            }
        }
    }
}
