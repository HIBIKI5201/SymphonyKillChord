using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    public class MissionHudPresenter
    {
        public MissionHudPresenter(MissionRuntimeService missionRuntimeService, IMissionHudViewModel missionHudViewModel)
        {
            _missionRuntimeService = missionRuntimeService;
            _missionHudViewModel = missionHudViewModel;
        }

        public void Present()
        {
            MissionEvaluationResult result = _missionRuntimeService.BuildEvaluationResult();

            MissionEvaluationItemDTO[] evaluationItems = new MissionEvaluationItemDTO[result.Progresses.Length];

            for(int i = 0; i < evaluationItems.Length; i++)
            {
                MissionEvaluationProgress progress = result.Progresses[i];
                evaluationItems[i] = new MissionEvaluationItemDTO(
                    progress.Description,
                    progress.IsAchieved);
            }

            string resultText = _missionRuntimeService.MissionProgress.EndReason.ToString();

            MissionHudDTO dto = new MissionHudDTO(
                _missionRuntimeService.MissionDefinition.MainMissionText,
                resultText,
                evaluationItems);

            _missionHudViewModel.Apply(dto);
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly IMissionHudViewModel _missionHudViewModel;
    }
}
