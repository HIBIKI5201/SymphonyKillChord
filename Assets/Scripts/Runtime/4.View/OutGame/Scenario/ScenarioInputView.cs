using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.Persistent.Input;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class ScenarioInputView : MonoBehaviour
    {
        public void Initilize(InputController inputController)
        {
            _inputController = inputController;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                _inputController.MouseClick();
            }
        }
        private InputController _inputController;
    }
}
