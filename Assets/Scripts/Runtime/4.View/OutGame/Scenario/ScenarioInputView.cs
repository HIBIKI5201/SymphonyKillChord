using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class ScenarioInputView : MonoBehaviour
    {
        [SerializeField]
        private KeyCode _advanceKey = KeyCode.Mouse0;
        [SerializeField]
        private KeyCode _fastForwardKey = KeyCode.LeftShift;
        [SerializeField]
        private KeyCode _pauseToggleKey = KeyCode.Space;
        [SerializeField]
        private KeyCode _skipKey = KeyCode.Escape;

        public void Initialize(InputController inputController)
        {
            _inputController = inputController;
        }

        private void Update()
        {
            if (_inputController == null) return;

            if (Input.GetKeyDown(_advanceKey))
            {
                _inputController.MouseClick();
            }

            if (Input.GetKeyDown(_fastForwardKey))
            {
                _inputController.SetFastForward(true);
            }

            if (Input.GetKeyUp(_fastForwardKey))
            {
                _inputController.SetFastForward(false);
            }

            if (Input.GetKeyDown(_pauseToggleKey))
            {
                _inputController.TogglePause();
            }

            if (Input.GetKeyDown(_skipKey))
            {
                _inputController.Skip();
            }
        }

        private InputController _inputController;
    }
}
