using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KillChord.Runtime.View
{
    public class MobileInput : MonoBehaviour
    {
        private void Initialize(PlayerInputView playerInputView)
        {
            _initialized = true;
            _playerInputView = playerInputView;
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            if (!_initialized) return;

            foreach (var touch in Touch.activeTouches)
            {
                
            }
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }

        private bool _initialized = false;
        private PlayerInputView _playerInputView;
    }
}