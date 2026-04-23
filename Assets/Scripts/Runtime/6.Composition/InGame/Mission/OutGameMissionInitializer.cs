using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.View;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class OutGameMissionInitializer : MonoBehaviour
    {
        [SerializeField] private OutGameMissionButtonView[] _buttons;

        private void Awake()
        {
            SelectedMissionState selectedMissionState = new SelectedMissionState();
            OutGameMissionSelectController controller = new OutGameMissionSelectController(selectedMissionState);

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Initialize(controller);
            }

            ServiceLocator.RegisterInstance(selectedMissionState);
        }
    }
}
