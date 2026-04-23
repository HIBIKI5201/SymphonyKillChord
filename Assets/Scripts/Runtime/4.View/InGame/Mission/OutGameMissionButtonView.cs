using KillChord.Runtime.Adaptor.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class OutGameMissionButtonView : MonoBehaviour
    {
        public void Initialize(OutGameMissionSelectController controller)
        {
            _controller = controller;
        }

        public void OnClick()
        {
            _controller.Select(_missionId);
        }

        [SerializeField] private string _missionId;
        private OutGameMissionSelectController _controller;
    }
}
