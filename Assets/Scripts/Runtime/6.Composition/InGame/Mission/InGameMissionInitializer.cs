using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.InfraStructure.InGame.Mission;
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
        /// <summary> ミッションカタログアセット。 </summary>
        [SerializeField, Tooltip("ミッション定義を保持するカタログアセット。")] private MissionCatalogAsset _missionCatalogAsset;
        /// <summary> ミッションHUDのビュー。 </summary>
        [SerializeField, Tooltip("ミッション情報を表示するHUDのビュー。")] private MissionHudView _missionHudView;
        /// <summary> ミッションループのビュー。 </summary>
        [SerializeField, Tooltip("ミッションの更新処理を行うループのビュー。")] private MissionLoopView _missionLoopView;

        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        public void Initialize()
        {
            SelectedMissionState selectedMissionState =
                ServiceLocator.GetInstance<SelectedMissionState>();

            IMissionDefinitionRepository missionDefinitionRepository =
                new MissionDefinitionRepository(_missionCatalogAsset);

            MissionFactory missionFactory = new MissionFactory();
            InGameMissionController controller = new InGameMissionController(
                selectedMissionState, missionDefinitionRepository, missionFactory);

            MissionDefinition definition = controller.LoadDefinition();
            MissionProgress progress = controller.CreateProgress();

            MissionRuntimeService missionRuntimeService = new MissionRuntimeService(
                definition,
                progress,
                new MissionTimeAdvanceUsecase(),
                new MissionEnemyKilledUsecase(),
                new MissionPlayerDeadUsecase(),
                new MissionRuleRunner(definition),
                new MissionEvaluationRunner());

            MissionHudViewModel missionHudViewModel = new MissionHudViewModel();

            MissionHudPresenter missionHudPresenter = new MissionHudPresenter(
                missionRuntimeService, missionHudViewModel);

            MissionEventController missionEventController = new MissionEventController(missionRuntimeService,
                missionHudPresenter);

            _missionHudView.Initialize(missionHudViewModel);
            _missionLoopView.Initialize(missionEventController);

            missionHudPresenter.Present();

            ServiceLocator.RegisterInstance(missionRuntimeService);
            ServiceLocator.RegisterInstance(missionEventController);
        }
    }
}
