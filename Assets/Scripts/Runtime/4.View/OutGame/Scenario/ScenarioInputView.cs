using KillChord.Runtime.Adaptor.OutGame.Scenario;
using UnityEngine;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    /// プレイヤー入力をシナリオ操作へ変換する入力ビュー。
    /// </summary>
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

        /// <summary>
        /// 依存先を受け取りシナリオ表示を初期化する。
        /// </summary>
        public void Initialize(InputController inputController)
        {
            _inputController = inputController;
        }

        /// <summary>
        /// 毎フレームの入力監視または演出更新を行う。
        /// </summary>
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