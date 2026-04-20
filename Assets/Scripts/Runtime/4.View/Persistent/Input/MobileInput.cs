using System;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

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
        }

        private void OnDestroy()
        {
            
        }

        private bool _initialized = false;
        private PlayerInputView _playerInputView;
    }
}