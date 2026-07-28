using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Composition.OutGame.Screen
{
    /// <summary>
    ///     画面 ID と View の対応表クラス。
    /// </summary>
    public sealed class ScreenViewRegistry : IScreenViewRegistry, IScreenLifecycleSignal, IDisposable
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

        /// <inheritdoc />
        public event Action<ScreenId> OnScreenWillShow;

        /// <summary>
        ///    指定画面を即時表示します。
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="targetSceneName"></param>
        public void ShowImmediately(ScreenId screenId, string targetSceneName = null)
        {
            ScreenViewBase view = _views[screenId];
            PrepareForShow(screenId, view, targetSceneName);
            view.ShowImmediately();
        }

        /// <summary>
        ///    指定画面を即時非表示にします。
        /// </summary>
        public void HideImmediately(ScreenId screenId)
        {
            _views[screenId].HideImmediately();
        }

        /// <summary>
        ///     指定画面を表示し、トランジションの完了を待機します。
        /// </summary>
        public async Task Show(ScreenId screenId, CancellationToken token, string targetSceneName = null)
        {
            ScreenViewBase view = _views[screenId];
            PrepareForShow(screenId, view, targetSceneName);
            await view.Show(token);
        }

        /// <summary>
        ///     指定画面を非表示にし、トランジションの完了を待機します。
        /// </summary>
        public async Task Hide(ScreenId screenId, CancellationToken token)
        {
            await _views[screenId].Hide(token);
        }

        /// <summary>
        ///     全画面を即時非表示にします。
        /// </summary>
        public void HideAllImmediately()
        {
            foreach (IScreenView screenView in _views.Values)
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

        /// <summary>
        ///     画面固有の表示準備を行い、表示直前を通知します。
        /// </summary>
        private void PrepareForShow(
            ScreenId screenId,
            ScreenViewBase view,
            string targetSceneName)
        {
            if (view is BattlePreparationScreen battlePreparationScreen
                && !string.IsNullOrEmpty(targetSceneName))
            {
                battlePreparationScreen.SetTargetSceneName(targetSceneName);
            }

            OnScreenWillShow?.Invoke(screenId);
        }

        private readonly IReadOnlyDictionary<ScreenId, ScreenViewBase> _views;
    }
}
