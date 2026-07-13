using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.View.InGame.Mission;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Mission
{
    /// <summary>
    ///     インゲームにおけるミッションシステムの初期化を行うクラス。
    /// </summary>
    public class InGameMissionInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(InGameMissionInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 300;

        /// <summary>
        ///     ミッションシステムを構築してContainerを登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (!TryInitialize(out MissionRuntimeService missionRuntimeService))
            {
                return false;
            }

            MissionEventController missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            if (missionEventController == null)
            {
                Debug.LogError($"[{nameof(InGameMissionInitializer)}] {nameof(MissionEventController)} を取得できませんでした。", this);
                return false;
            }

            _moduleContainer = new MissionModuleContainer(missionRuntimeService, missionEventController);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isModuleRegistered = true;
            return true;
        }

        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        public bool TryInitialize(out MissionRuntimeService missionRuntimeService)
        {
            missionRuntimeService = null;

            if (!ValidateReferences())
            {
                return false;
            }

            if (!ServiceLocator.TryGetInstance(
            out SelectedMissionState selectedMissionState))
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    $"{nameof(SelectedMissionState)}を取得できませんでした。",
                    this);

                return false;
            }

            if (!selectedMissionState.HasSelectedMission)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    "OutGameでミッションが選択されていません。",
                    this);

                return false;
            }

            MissionDefinition definition = selectedMissionState.CurrentMissionDefinition;
            MissionProgress progress = new MissionFactory().CreateMissionProgress();

            missionRuntimeService = new MissionRuntimeService(
                definition,
                progress,
                new MissionTimeAdvanceUsecase(),
                new MissionEnemyKilledUsecase(),
                new MissionPlayerDeadUsecase(),
                new MissionRuleRunner(definition),
                new MissionEvaluationRunner());

            MissionHudViewModel missionHudViewModel = new MissionHudViewModel();

            MissionHudPresenter missionHudPresenter = new MissionHudPresenter(
                missionRuntimeService,
                missionHudViewModel);

            MissionEventController missionEventController = new MissionEventController(
                missionRuntimeService,
                missionHudPresenter);

            _missionHudView.Initialize(missionHudViewModel);
            _missionLoopView.Initialize(missionEventController);

            missionHudPresenter.Present();

            ServiceLocator.RegisterInstance(missionRuntimeService);
            ServiceLocator.RegisterInstance(missionEventController);

            _registeredMissionRuntimeService = true;
            _registeredMissionEventController = true;

            return true;
        }

        /// <summary>
        ///     登録済みContainerを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isModuleRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<MissionModuleContainer>();
            _moduleContainer = null;
            _isModuleRegistered = false;
        }

        [SerializeField, Tooltip("ミッション情報を表示するHUDのビュー。")] private MissionHudView _missionHudView;
        [SerializeField, Tooltip("ミッションの更新処理を行うループのビュー。")] private MissionLoopView _missionLoopView;

        private bool _registeredMissionRuntimeService;
        private bool _registeredMissionEventController;
        private bool _isModuleRegistered;
        private MissionModuleContainer _moduleContainer;

        private void OnDestroy()
        {
            if (_registeredMissionRuntimeService)
            {
                ServiceLocator.UnregisterInstance<MissionRuntimeService>();
                _registeredMissionRuntimeService = false;
            }

            if (_registeredMissionEventController)
            {
                ServiceLocator.UnregisterInstance<MissionEventController>();
                _registeredMissionEventController = false;
            }
        }

        /// <summary>
        ///     Inspector参照を検証します。
        /// </summary>
        /// <returns> 参照が有効な場合はtrue。 </returns>
        private bool ValidateReferences()
        {
            if (_missionHudView == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    $"{nameof(_missionHudView)}が設定されていません。",
                    this);

                return false;
            }

            if (_missionLoopView == null)
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] " +
                    $"{nameof(_missionLoopView)}が設定されていません。",
                    this);

                return false;
            }

            return true;
        }
    }
}
