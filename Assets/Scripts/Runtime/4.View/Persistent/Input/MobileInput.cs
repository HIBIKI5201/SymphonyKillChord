using KillChord.Runtime.Utility.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KillChord.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     モバイルの視点操作領域からドラッグとフリック入力を通知する。
    /// </summary>
    public class MobileInput : MonoBehaviour
    {
        [SerializeField, Tooltip("視点操作領域のUIレイキャストに使用するGraphicRaycaster。")]
        private GraphicRaycaster _rayCaster;

        [SerializeField, Min(0f), Tooltip("フリックとして扱うタッチ開始から終了までの最大秒数。")]
        private float _flickMaxDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("フリックとして扱う画面上の最小移動距離。")]
        private float _flickMinDistance = 80f;

        private PointerEventData _eventData;
        private bool _initialized = false;
        private bool _isTracking = false;

        private Vector2 _legacyPosition;
        private Vector2 _touchStartPosition;
        private float _touchStartTime;
        private int _trackedTouchID;

        private PlayerInputView _playerInputView;
        private EventSystem _eventSystem;

        private readonly List<RaycastResult> _raycastResults = new();

        /// <summary>
        ///     入力通知先を設定し、Enhanced Touchを有効化する。
        /// </summary>
        /// <param name="playerInputView"> モバイル入力の通知先。 </param>
        public void Initialize(PlayerInputView playerInputView)
        {
            _initialized = true;
            _playerInputView = playerInputView;

            _eventSystem = EventSystem.current;
            _eventData = new PointerEventData(_eventSystem);

            EnhancedTouchSupport.Enable();

#if UNITY_EDITOR
            //Unity上ではクリックをタッチとしてシミュレーションを有効化する必要がある
            TouchSimulation.Enable();
#endif

            DevLog.Log("MobileInput Initialize");
        }

        /// <summary>
        ///     タッチ入力を追跡し、視点操作の入力として通知する。
        /// </summary>
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
                    Vector2 endPosition = trackingTouch.HasValue
                        ? trackingTouch.Value.screenPosition
                        : _legacyPosition;
                    _isTracking = false;
                    _trackedTouchID = -1;

                    OnTrackedTouchCanceled(endPosition);
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

        /// <summary>
        ///     タッチ開始時に視点操作を開始し、開始位置と時刻を記録する。
        /// </summary>
        private void OnTrackedTouchBegan(Vector2 screenPos)
        {
            _playerInputView.OnMobileLook(InputActionPhase.Started, Vector2.zero);
            _legacyPosition = screenPos;
            _touchStartPosition = screenPos;
            _touchStartTime = Time.unscaledTime;
        }

        /// <summary>
        ///     タッチの移動量を視点操作の入力として通知する。
        /// </summary>
        private void OnTrackedTouchMoved(Vector2 screenPos)
        {
            Vector2 delta = screenPos - _legacyPosition;

            _playerInputView.OnMobileLook(InputActionPhase.Performed, delta);
            _legacyPosition = screenPos;
        }

        /// <summary>
        ///     タッチ終了時に視点操作を終える。
        ///     短時間で一定距離以上動いた場合はフリックとみなし、ロックオン対象を切り替える。
        /// </summary>
        private void OnTrackedTouchCanceled(Vector2 screenPos)
        {
            _playerInputView.OnMobileLook(InputActionPhase.Canceled, Vector2.zero);

            float duration = Time.unscaledTime - _touchStartTime;
            Vector2 displacement = screenPos - _touchStartPosition;
            float minimumDistanceSquared = _flickMinDistance * _flickMinDistance;
            if (duration <= _flickMaxDuration && displacement.sqrMagnitude >= minimumDistanceSquared)
            {
                _playerInputView.OnMobileLockOnSelect(Mathf.Sign(displacement.x));
            }
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

        /// <summary>
        ///     拡張タッチ機能を無効にする。
        /// </summary>
        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }
    }
}
