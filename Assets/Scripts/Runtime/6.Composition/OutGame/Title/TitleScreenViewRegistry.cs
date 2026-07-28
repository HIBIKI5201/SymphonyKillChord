using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        ///     指定された画面 ID の View を表示します。
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="token"></param>
        /// <param name="targetSceneName"></param>
        /// <returns></returns>
        public async Task Show(ScreenId screenId, CancellationToken token, string targetSceneName = null)
        {
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            await view.Show(token);
        }

        /// <summary>
        ///    指定された画面 ID の View を非表示にします。
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Hide(ScreenId screenId, CancellationToken token)
        {
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            await view.Hide(token);
        }

        /// <summary>
        ///    指定された画面 ID の View を即座に表示します。
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="targetSceneName"></param>
        public void ShowImmediately(ScreenId screenId, string targetSceneName = null)
        {
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            view.ShowImmediately();
        }

        /// <summary>
        ///     指定された画面 ID の View を即座に非表示にします。
        /// </summary>
        /// <param name="screenId"></param>
        public void HideImmediately(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out var view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            view.HideImmediately();
        }

        /// <summary>
        ///   すべての View を即座に非表示にします。
        /// </summary>
        public void HideAllImmediately()
        {
            foreach (IScreenView view in _views.Values)
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
