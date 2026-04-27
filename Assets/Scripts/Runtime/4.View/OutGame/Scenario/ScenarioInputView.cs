using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class ScenarioInputView : MonoBehaviour
    {
        public void Initialize(InputController inputController)
        {
            _inputController = inputController;
        }

        private void Update()
        {
            if (_inputController == null) return;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                _inputController.MouseClick();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _inputController.SetFastForward(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                _inputController.SetFastForward(false);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _inputController.TogglePause();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _inputController.Skip();
            }
        }
        private InputController _inputController;
    }
}
