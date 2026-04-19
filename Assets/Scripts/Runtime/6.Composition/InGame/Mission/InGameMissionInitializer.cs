using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class InGameMissionInitializer : MonoBehaviour
    {
        [SerializeField] private MissionCatalogAsset _missionCatalogAsset;
        [SerializeField] private MissionHudView _missionHudView;
        [SerializeField] private MissionLoopView _missionLoopView;

        private void Awake()
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
