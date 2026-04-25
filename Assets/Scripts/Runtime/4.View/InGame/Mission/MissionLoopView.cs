using KillChord.Runtime.Adaptor.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class MissionLoopView : MonoBehaviour
    {
        public void Initialize(MissionEventController missionEventController)
        {
            _missionEventController = missionEventController;
        }

        private void Update()
        {
            _missionEventController?.Tick(Time.deltaTime);
        }

        private MissionEventController _missionEventController;
    }
}
