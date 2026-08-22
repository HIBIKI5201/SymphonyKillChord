using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.Persistent.Music;
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
            _isDisposed = true;
        }

        private const string DRAG_COMPLETED_CLASS_NAME = "drag-just-completed";
        private const string SELECT_SOUND_TAB_CLASS_NAME = "tab-se-select";

        private readonly Dictionary<VisualElement, Tab> _tabByHeader = new();
        private readonly VisualElement _root;
        private readonly UISoundEffectConfig _config;
        private readonly IPlayableAudioSource _player;
        private bool _isDisposed;

        /// <summary>
        ///     ClickEventに対応するCueを解決して再生する。
        /// </summary>
        /// <param name="evt"> 観測したClickEvent。 </param>
        private void HandleClickedHandler(ClickEvent evt)
        {
            if (evt.target is not VisualElement target)
            {
                return;
            }

            ProcessClick(target);
        }

        /// <summary>
        ///     PointerDownEventに明示されたCueを解決して再生する。
        /// </summary>
        /// <param name="evt"> 観測したPointerDownEvent。 </param>
        private void HandlePointerDownHandler(PointerDownEvent evt)
        {
            if (evt.target is not VisualElement target)
            {
                return;
            }

            ProcessPointerDown(target);
        }

        /// <summary>
        ///     指定された要素からClickEvent用のCueを解決して再生する。
        /// </summary>
        /// <param name="target"> ClickEventのtarget要素。 </param>
        internal void ProcessClick(VisualElement target)
        {
            if (_isDisposed || target == null)
            {
                return;
            }

            Button nearestButton = null;
            Tab nearestTab = null;
            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (element.ClassListContains(DRAG_COMPLETED_CLASS_NAME))
                {
                    return;
                }

                if (!element.enabledInHierarchy)
                {
                    return;
                }

                if (_config.TryResolveClickCue(element, out string cueName, out bool hasMultipleMatches))
                {
                    _player.Play(cueName);
                    return;
                }

                if (hasMultipleMatches)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIToolkitSoundEffectBinder)}] " +
                        $"要素{element.name}にClickEvent用の音クラスが複数設定されています。");
                    return;
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
                if (nearestTab.ClassListContains(SELECT_SOUND_TAB_CLASS_NAME)
                    && _config.TryGetCue(UISoundEffectKind.Select, out string cueName))
                {
                    _player.Play(cueName);
                    return;
                }

                _player.Play(_config.DefaultButtonClickCue);
                return;
            }

            if (nearestButton != null && nearestButton.enabledInHierarchy)
            {
                _player.Play(_config.DefaultButtonClickCue);
            }
        }

        /// <summary>
        ///     指定された要素からPointerDownEvent用のCueを解決して再生する。
        /// </summary>
        /// <param name="target"> PointerDownEventのtarget要素。 </param>
        internal void ProcessPointerDown(VisualElement target)
        {
            if (_isDisposed || target == null)
            {
                return;
            }

            bool isButtonOrDescendant = IsButtonOrDescendant(target);
            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (!element.enabledInHierarchy)
                {
                    return;
                }

                if (_config.TryResolvePointerDownCue(
                    element,
                    out string cueName,
                    out bool hasMultipleMatches))
                {
                    if (isButtonOrDescendant)
                    {
                        Debug.LogWarning(
                            $"[{nameof(UIToolkitSoundEffectBinder)}] " +
                            $"Buttonまたはその子要素{element.name}に" +
                            "PointerDownEvent用の音クラスが設定されています。");
                        return;
                    }

                    _player.Play(cueName);
                    return;
                }

                if (hasMultipleMatches)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIToolkitSoundEffectBinder)}] " +
                        $"要素{element.name}にPointerDownEvent用の音クラスが複数設定されています。");
                    return;
                }

                if (ReferenceEquals(element, _root))
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     指定要素自身または祖先がButtonか確認する。
        /// </summary>
        /// <param name="target"> 確認を開始する要素。 </param>
        /// <returns> Button自身またはその子要素の場合はtrue。 </returns>
        private bool IsButtonOrDescendant(VisualElement target)
        {
            for (VisualElement element = target; element != null; element = element.parent)
            {
                if (element is Button)
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
