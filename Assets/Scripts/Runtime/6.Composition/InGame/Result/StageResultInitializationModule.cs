using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Result;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Result
{
    /// <summary>
    ///     リザルト表示を初期化して公開するモジュールです。
    /// </summary>
    public sealed class StageResultInitializationModule : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(StageResultInitializationModule);

        /// <summary> 実行順です。 </summary>
        public override int Order => 400;

        /// <summary>
        ///     リザルトViewを解決してContainerを登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            StageResultView view = FindFirstObjectByType<StageResultView>();
            if (view == null)
            {
                Debug.LogError($"[{nameof(StageResultInitializationModule)}] {nameof(StageResultView)} が見つかりません。", this);
                return false;
            }

            _container = new StageResultModuleContainer(view);
            ServiceLocator.RegisterInstance(_container);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     ミッションとシーン遷移に結合してリザルト表示を構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            MissionModuleContainer missionContainer = ServiceLocator.GetInstance<MissionModuleContainer>();
            if (missionContainer == null)
            {
                Debug.LogError($"[{nameof(StageResultInitializationModule)}] {nameof(MissionModuleContainer)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState))
            {
                Debug.LogError($"[{nameof(StageResultInitializationModule)}] {nameof(SelectedBattleStageState)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SelectedMissionState selectedMissionState))
            {
                Debug.LogError($"[{nameof(StageResultInitializationModule)}] {nameof(SelectedMissionState)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionUsecase sceneTransitionUsecase))
            {
                Debug.LogError($"[{nameof(StageResultInitializationModule)}] {nameof(SceneTransitionUsecase)} が見つかりません。", this);
                return false;
            }

            StageResultViewModel viewModel = new();
            _container.Presenter = new StageResultPresenter(
                missionContainer.MissionRuntimeService,
                selectedBattleStageState,
                viewModel);
            _container.Controller = new StageResultController(
                sceneTransitionUsecase,
                selectedBattleStageState,
                selectedMissionState);
            _container.View.Initialize(viewModel, _container.Controller);
            return true;
        }

        /// <summary>
        ///     登録済みContainerを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<StageResultModuleContainer>();
            _container = null;
            _isRegistered = false;
        }

        private StageResultModuleContainer _container;
        private bool _isRegistered;
    }
}
