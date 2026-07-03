using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.View.InGame.Mission;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Mission
{
    /// <summary>
    ///     インゲームにおけるミッションシステムの初期化を行うクラス。
    /// </summary>
    public class InGameMissionInitializer : MonoBehaviour
    {
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

        [SerializeField, Tooltip("ミッション情報を表示するHUDのビュー。")] private MissionHudView _missionHudView;
        [SerializeField, Tooltip("ミッションの更新処理を行うループのビュー。")] private MissionLoopView _missionLoopView;

        private bool _registeredMissionRuntimeService;
        private bool _registeredMissionEventController;

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
