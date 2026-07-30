using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            view.Show();
        }

        /// <summary>
        ///     指定された画面 ID の View を非表示状態にします。
        /// </summary>
        /// <param name="screenId"></param>
        public void Hide(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            view.Hide();
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
            foreach (IDisposable disposable in _views.Values)
            {
                disposable?.Dispose();
            }
        }

        private readonly Dictionary<ScreenId, ScreenViewBase> _views;
    }
}
