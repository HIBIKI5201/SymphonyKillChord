using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace KillChord.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     仮想スティック上のポインター軌跡からフリック方向を通知するViewクラス。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(OnScreenStick))]
    public sealed class MobileStickFlickInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        /// <summary>
        ///     フリック入力の通知先を設定する。
        /// </summary>
        /// <param name="playerInputView"> フリック入力の通知先。 </param>
        public void Initialize(PlayerInputView playerInputView)
        {
            _playerInputView = playerInputView;
            CacheReferences();
            ResetTracking();
        }

        /// <summary>
        ///     通知先と追跡中の入力を解除する。
        /// </summary>
        public void Uninitialize()
        {
            _playerInputView = null;
            ResetTracking();
        }

        /// <summary>
        ///     追跡中のポインター位置を記録する。
        /// </summary>
        /// <param name="eventData"> ドラッグイベント情報。 </param>
        public void OnDrag(PointerEventData eventData)
        {
            if (!IsTrackedPointer(eventData))
            {
                return;
            }

            UpdateFarthestPosition(eventData);
        }

        /// <summary>
        ///     フリック候補となるポインターの追跡を開始する。
        /// </summary>
        /// <param name="eventData"> ポインター押下イベント情報。 </param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isTracking || eventData == null)
            {
                return;
            }

            if (!TryGetCanvasLocalPosition(eventData, out Vector2 localPosition))
            {
                return;
            }

            _trackedPointerId = eventData.pointerId;
            _touchStartPosition = localPosition;
            _farthestPosition = localPosition;
            _farthestDistanceSquared = 0f;
            _touchStartTime = Time.unscaledTime;
            _isTracking = true;
        }

        /// <summary>
        ///     ポインター解放時にフリックを判定して方向を通知する。
        /// </summary>
        /// <param name="eventData"> ポインター解放イベント情報。 </param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsTrackedPointer(eventData))
            {
                return;
            }

            UpdateFarthestPosition(eventData);

            float duration = Time.unscaledTime - _touchStartTime;
            Vector2 displacement = _farthestPosition - _touchStartPosition;
            float movementRange = _onScreenStick != null
                ? _onScreenStick.movementRange
                : 0f;

            ResetTracking();

            if (_playerInputView == null
                || movementRange <= float.Epsilon
                || duration > _maximumFlickDuration
                || displacement.sqrMagnitude <= float.Epsilon)
            {
                return;
            }

            float minimumDistance = movementRange * _minimumFlickDistanceRate;
            if (displacement.sqrMagnitude < minimumDistance * minimumDistance)
            {
                return;
            }

            _playerInputView.OnMobileDodgeFlick(displacement.normalized);
        }

        private const int INVALID_POINTER_ID = int.MinValue;

        [SerializeField, Tooltip("フリック距離の正規化基準となるオンスクリーンスティック。")]
        private OnScreenStick _onScreenStick;

        [Header("フリック判定条件")]
        [SerializeField, Range(0f, 1f), Tooltip("フリックとして扱う最小距離をスティック可動範囲に対する割合で指定する。")]
        private float _minimumFlickDistanceRate = 0.8f;

        [SerializeField, Min(0f), Tooltip("フリックとして扱う押下から解放までの最大秒数。")]
        private float _maximumFlickDuration = 0.2f;

        private PlayerInputView _playerInputView;
        private RectTransform _interactionRectTransform;
        private bool _isTracking;
        private int _trackedPointerId = INVALID_POINTER_ID;
        private Vector2 _touchStartPosition;
        private Vector2 _farthestPosition;
        private float _farthestDistanceSquared;
        private float _touchStartTime;

        /// <summary>
        ///     無効化時に追跡中の入力を破棄する。
        /// </summary>
        private void OnDisable()
        {
            ResetTracking();
        }

        /// <summary>
        ///     同一GameObjectと仮想スティックの座標変換基準から必要な参照を取得する。
        /// </summary>
        private void CacheReferences()
        {
            if (_onScreenStick == null)
            {
                _onScreenStick = GetComponent<OnScreenStick>();
            }

            if (_interactionRectTransform == null && transform.parent != null)
            {
                _interactionRectTransform = transform.parent.GetComponentInParent<RectTransform>();
            }
        }

        /// <summary>
        ///     指定されたイベントが現在追跡中のポインターか判定する。
        /// </summary>
        /// <param name="eventData"> 判定するイベント情報。 </param>
        /// <returns> 追跡中のポインターである場合はtrue。 </returns>
        private bool IsTrackedPointer(PointerEventData eventData)
        {
            return _isTracking
                && eventData != null
                && eventData.pointerId == _trackedPointerId;
        }

        /// <summary>
        ///     開始位置から最も離れたポインター位置を更新する。
        /// </summary>
        /// <param name="eventData"> 更新に使用するイベント情報。 </param>
        private void UpdateFarthestPosition(PointerEventData eventData)
        {
            if (!TryGetCanvasLocalPosition(eventData, out Vector2 localPosition))
            {
                return;
            }

            float distanceSquared = (localPosition - _touchStartPosition).sqrMagnitude;
            if (distanceSquared <= _farthestDistanceSquared)
            {
                return;
            }

            _farthestPosition = localPosition;
            _farthestDistanceSquared = distanceSquared;
        }

        /// <summary>
        ///     画面座標をOnScreenStickと共通のローカル座標へ変換する。
        /// </summary>
        /// <param name="eventData"> 座標を持つイベント情報。 </param>
        /// <param name="localPosition"> 変換後のCanvasローカル座標。 </param>
        /// <returns> 変換できた場合はtrue。 </returns>
        private bool TryGetCanvasLocalPosition(PointerEventData eventData, out Vector2 localPosition)
        {
            localPosition = Vector2.zero;
            if (eventData == null || _interactionRectTransform == null)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _interactionRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPosition);
        }

        /// <summary>
        ///     ポインター追跡状態を初期値へ戻す。
        /// </summary>
        private void ResetTracking()
        {
            _isTracking = false;
            _trackedPointerId = INVALID_POINTER_ID;
            _touchStartPosition = Vector2.zero;
            _farthestPosition = Vector2.zero;
            _farthestDistanceSquared = 0f;
            _touchStartTime = 0f;
        }
    }
}
