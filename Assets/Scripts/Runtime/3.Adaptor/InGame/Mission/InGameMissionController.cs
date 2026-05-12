using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    public class InGameMissionController
    {
        public InGameMissionController(
            SelectedMissionState selectedMissionState,
            IMissionDefinitionRepository missionDefinitionRepository,
            MissionFactory missionFactory)
        {
            _selectedMissionState = selectedMissionState;
            _missionDefinitionRepository = missionDefinitionRepository;
            _missionFactory = missionFactory;
        }

        public MissionDefinition LoadDefinition()
        {
            return _missionDefinitionRepository.Get(_selectedMissionState.CurrentMissionId);
        }

        public MissionProgress CreateProgress()
        {
            return _missionFactory.CreateMissionProgress();
        }

        private readonly SelectedMissionState _selectedMissionState;
        private readonly IMissionDefinitionRepository _missionDefinitionRepository;
        private readonly MissionFactory _missionFactory;
    }
}
