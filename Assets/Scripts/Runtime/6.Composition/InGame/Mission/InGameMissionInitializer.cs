using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
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
        public override int Order => 600;

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
        ///     プレイヤー戦闘イベントとミッション実績記録を結合します。
        /// </summary>
        /// <returns> 結合に成功した場合はtrueです。 </returns>
        public override bool Ready()
        {
            PlayerModuleContainer playerModuleContainer =
                ServiceLocator.GetInstance<PlayerModuleContainer>();
            SkillModuleContainer skillModuleContainer =
                ServiceLocator.GetInstance<SkillModuleContainer>();
            if (playerModuleContainer == null
                || playerModuleContainer.PlayerEntity == null
                || playerModuleContainer.PlayerController == null
                || playerModuleContainer.PlayerAttackController == null
                || skillModuleContainer?.SkillController == null
                || !ServiceLocator.TryGetInstance(out TargetSystemController targetSystemController))
            {
                Debug.LogError(
                    $"[{nameof(InGameMissionInitializer)}] プレイヤー戦闘モジュールを取得できませんでした。",
                    this);
                return false;
            }

            MissionProgressRecorderController recorderController =
                new MissionProgressRecorderController(
                    _moduleContainer.MissionRuntimeService.MissionProgress,
                    _moduleContainer.MissionEventController);
            recorderController.Bind(
                playerModuleContainer.PlayerEntity,
                playerModuleContainer.PlayerController,
                playerModuleContainer.PlayerAttackController,
                skillModuleContainer.SkillController,
                targetSystemController);
            _recorderController = recorderController;

            if (_missionStepPopupView != null)
            {
                _popupController = new MissionStepPopupController(
                    _moduleContainer.MissionRuntimeService,
                    _moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition,
                    _missionStepPopupView,
                    playerModuleContainer.InputSuppressionState,
                    _popupInputSuppressionDuration);
            }

            _playerBuffController = new MissionPlayerBuffController(
                _moduleContainer.MissionRuntimeService,
                _moduleContainer.MissionRuntimeService.MissionDefinition.ClearCondition,
                playerModuleContainer.PlayerEntity);

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
                new MissionActionPerformedUsecase(),
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
            _recorderController?.Dispose();
            _popupController?.Dispose();
            _playerBuffController?.Dispose();

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
        [SerializeField, Tooltip("目標ステップの説明ポップアップを表示するビュー。未設定の場合はポップアップ機能を使用しない。")] private MissionStepPopupView _missionStepPopupView;
        [SerializeField, Min(0f), Tooltip("説明ポップアップ表示直後にプレイヤー入力を無効化する秒数。")] private float _popupInputSuppressionDuration = MissionStepPopupController.DefaultInputSuppressionDuration;

        private bool _registeredMissionRuntimeService;
        private bool _registeredMissionEventController;
        private bool _isModuleRegistered;
        private MissionModuleContainer _moduleContainer;
        private MissionProgressRecorderController _recorderController;
        private MissionStepPopupController _popupController;
        private MissionPlayerBuffController _playerBuffController;

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
