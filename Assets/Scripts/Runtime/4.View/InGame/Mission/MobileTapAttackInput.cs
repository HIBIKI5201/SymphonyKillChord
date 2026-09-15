using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     スマートフォンで説明ポップアップ表示中に、画面のタップを攻撃入力として扱うViewクラス。
    ///     <para>
    ///         ポップアップ表示中は他のCanvasにタップを取られて攻撃ボタンが押せなくなるため、
    ///         UIの上に判定を重ねるのではなく、タッチを直接読み取って攻撃入力を送ります。
    ///         UIのレイキャストを遮らないので、仮想スティックやボタンなどの操作はそのまま使えます。
    ///     </para>
    /// </summary>
    public sealed class MobileTapAttackInput : MonoBehaviour
    {
        /// <summary>
        ///     タップ攻撃入力を生成します。生成直後は無効です。
        /// </summary>
        /// <param name="playerInputView"> 攻撃入力の送信先です。 </param>
        /// <returns> 生成したタップ攻撃入力です。 </returns>
        public static MobileTapAttackInput Create(PlayerInputView playerInputView)
        {
            if (playerInputView == null)
            {
                throw new ArgumentNullException(nameof(playerInputView));
            }

            GameObject inputObject = new GameObject(nameof(MobileTapAttackInput));
            MobileTapAttackInput input = inputObject.AddComponent<MobileTapAttackInput>();
            input._playerInputView = playerInputView;
            return input;
        }

        /// <summary>
        ///     タップによる攻撃入力の受け付けを開始します。
        /// </summary>
        public void Activate()
        {
            _isActive = true;
        }

        /// <summary>
        ///     タップによる攻撃入力の受け付けを終了します。押下中のタッチには解放を通知します。
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            ReleaseTrackedTouches();
        }

        /// <summary>
        ///     生成したオブジェクトを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (this == null)
            {
                return;
            }

            Deactivate();
            Destroy(gameObject);
        }

        private readonly HashSet<int> _trackedTouchIds = new();
        private readonly List<RaycastResult> _raycastResults = new();

        private PlayerInputView _playerInputView;
        private PointerEventData _pointerEventData;
        private EventSystem _pointerEventSystem;
        private bool _isActive;

        private void Update()
        {
            if (!_isActive || _playerInputView == null)
            {
                return;
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                int touchId = touch.touchId.ReadValue();
                if (touch.press.wasPressedThisFrame)
                {
                    // 仮想スティックやボタンなどの操作UIへのタップは、その操作に任せる。
                    if (IsOverInteractiveUi(touch.position.ReadValue()))
                    {
                        continue;
                    }

                    _trackedTouchIds.Add(touchId);
                    _playerInputView.OnMobileButton(InputActionKind.Attack, InputActionPhase.Started, 1f);
                }
                else if (touch.press.wasReleasedThisFrame && _trackedTouchIds.Remove(touchId))
                {
                    _playerInputView.OnMobileButton(InputActionKind.Attack, InputActionPhase.Canceled, 0f);
                }
            }
        }

        private void OnDisable()
        {
            ReleaseTrackedTouches();
        }

        /// <summary>
        ///     押下中として追跡しているタッチに解放を通知します。
        /// </summary>
        private void ReleaseTrackedTouches()
        {
            if (_trackedTouchIds.Count == 0)
            {
                return;
            }

            _trackedTouchIds.Clear();
            _playerInputView?.OnMobileButton(InputActionKind.Attack, InputActionPhase.Canceled, 0f);
        }

        /// <summary>
        ///     指定した画面座標が、仮想スティックやボタンなどの操作UIの上にあるかを調べます。
        /// </summary>
        /// <param name="screenPosition"> 調べる画面座標です。 </param>
        /// <returns> 操作UIの上にある場合はtrueです。 </returns>
        private bool IsOverInteractiveUi(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            // シーン遷移などで EventSystem が差し替わった場合は、イベントデータを作り直す。
            if (_pointerEventData == null || _pointerEventSystem != eventSystem)
            {
                _pointerEventData = new PointerEventData(eventSystem);
                _pointerEventSystem = eventSystem;
            }

            _pointerEventData.position = screenPosition;
            _raycastResults.Clear();
            eventSystem.RaycastAll(_pointerEventData, _raycastResults);

            for (int i = 0; i < _raycastResults.Count; i++)
            {
                GameObject hitObject = _raycastResults[i].gameObject;
                if (hitObject == null)
                {
                    continue;
                }

                if (hitObject.GetComponentInParent<OnScreenControl>() != null
                    || hitObject.GetComponentInParent<Selectable>() != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
