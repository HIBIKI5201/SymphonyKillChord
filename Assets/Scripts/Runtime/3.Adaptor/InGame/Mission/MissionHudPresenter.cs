using KillChord.Runtime.Application.InGame.Mission;

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
            string resultText = _missionRuntimeService.MissionProgress.EndReason.ToString();

            MissionHudDTO dto = new MissionHudDTO(
                _missionRuntimeService.MissionDefinition.MainMissionText,
                resultText);

            _missionHudViewModel.Apply(dto);
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly IMissionHudViewModel _missionHudViewModel;
    }
}
