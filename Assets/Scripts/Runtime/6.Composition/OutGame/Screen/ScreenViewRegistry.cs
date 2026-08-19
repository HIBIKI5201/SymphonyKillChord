using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
            }
            view.Show();
        }

        /// <summary>
        ///    指定画面を非表示状態にします。
        /// </summary>
        public void Hide(ScreenId screenId)
        {
            if (!_views.TryGetValue(screenId, out ScreenViewBase view))
            {
                Debug.LogWarning($"ScreenId {screenId} はレジストリに登録されていません。");
                return;
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
        }

        private readonly IReadOnlyDictionary<ScreenId, ScreenViewBase> _views;
    }
}
