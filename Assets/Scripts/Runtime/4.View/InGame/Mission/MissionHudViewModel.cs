using KillChord.Runtime.Adaptor.InGame.Mission;
using R3;

namespace KillChord.Runtime.View
{
    public class MissionHudViewModel : IMissionHudViewModel
    {
        public ReactiveProperty<string> MainMissionText { get; } = new(string.Empty);
        public ReactiveProperty<string> ResultText { get; } = new(string.Empty);

        public void Apply(in MissionHudDTO dto)
        {
            MainMissionText.Value = dto.MainMissionText;
            ResultText.Value = dto.ResultText;
        }
    }
}
