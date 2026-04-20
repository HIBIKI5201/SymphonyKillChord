using System.Collections.Generic;
using KillChord.Runtime.View.Persistent.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KillChord.Runtime.View
{
    public class MobileInput : MonoBehaviour
    {
        [SerializeField] private GraphicRaycaster _rayCaster;

        private PointerEventData _eventData;
        private bool _initialized = false;
        private bool _isTracking = false;

        private Vector2 _legacyPosition;
        private int _trackedTouchID;

        private PlayerInputView _playerInputView;
        private EventSystem _eventSystem;

        private readonly List<RaycastResult> _raycastResults = new();

        public void Initialize(PlayerInputView playerInputView)
        {
            _initialized = true;
            _playerInputView = playerInputView;

            _eventSystem = EventSystem.current;
            _eventData = new PointerEventData(_eventSystem);

            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            if (!_initialized) return;

            var touches = Touch.activeTouches;

            //一つの入力を追跡
            if (_isTracking)
            {
                Touch? trackingTouch = null;

                foreach (var t in touches)
                {
                    if (t.touchId == _trackedTouchID)
                    {
                        trackingTouch = t;
                        break;
                    }
                }

                //タッチの終了をここで検知
                if (!trackingTouch.HasValue || trackingTouch.Value.ended)
                {
                    _isTracking = false;
                    _trackedTouchID = -1;

                    OnTrackedTouchCanceled(Vector2.zero);
                    return;
                }

                Vector2 pos = trackingTouch.Value.screenPosition;
                OnTrackedTouchMoved(pos);

                return;
            }

            //新規のタッチがないか検出
            foreach (var touch in touches)
            {
                if (!touch.began) continue;

                Vector2 pos = touch.screenPosition;

                if (!IsInsideTouchArea(pos)) continue;

                _trackedTouchID = touch.touchId;
                _isTracking = true;

                OnTrackedTouchBegan(pos);
                break;
            }
        }

        private void OnTrackedTouchBegan(Vector2 screenPos)
        {
            _playerInputView.OnMobileLook(InputActionPhase.Started, Vector2.zero);
            _legacyPosition = screenPos;
        }

        private void OnTrackedTouchMoved(Vector2 screenPos)
        {
            Vector2 delta = screenPos - _legacyPosition;

            _playerInputView.OnMobileLook(InputActionPhase.Performed, delta);
            _legacyPosition = screenPos;
        }

        private void OnTrackedTouchCanceled(Vector2 screenPos)
        {
            _playerInputView.OnMobileLook(InputActionPhase.Canceled, Vector2.zero);
        }

        /// <summary>
        /// 入力範囲内にあるか、ボタンなどと被っていないかを調べる。
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        private bool IsInsideTouchArea(Vector2 screenPosition)
        {
            const string TAG_NAME = "TouchArea";

            _eventData.position = screenPosition;

            _raycastResults.Clear();
            _rayCaster.Raycast(_eventData, _raycastResults);

            if (_raycastResults.Count == 0)
                return false;

            return _raycastResults[0].gameObject != null &&
                   _raycastResults[0].gameObject.CompareTag(TAG_NAME);
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }
    }
}