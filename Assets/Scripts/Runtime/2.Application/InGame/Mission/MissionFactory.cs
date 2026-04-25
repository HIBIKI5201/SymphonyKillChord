using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Application
{
    public class MissionFactory
    {
        public MissionProgress CreateMissionProgress()
        {
            return new MissionProgress();
        }
    }
}
