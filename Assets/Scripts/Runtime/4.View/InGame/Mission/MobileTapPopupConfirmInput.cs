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
    ///     スマートフォンで説明ポップアップ表示中に、画面のタップをポップアップを閉じる入力として通知するViewクラス。
    ///     <para>
    ///         ポップアップ表示中は他のCanvasにタップを取られるため、UIの上に判定を重ねるのではなく、
    ///         タッチを直接読み取って通知します。
    ///         UIのレイキャストを遮らないので、仮想スティックやボタンなどの操作はそのまま使えます。
    ///     </para>
    /// </summary>
    public sealed class MobileTapPopupConfirmInput : MonoBehaviour
    {
        /// <summary> ポップアップ表示中に画面がタップされたことを通知するイベント。 </summary>
        public event Action OnTapped;

        /// <summary>
        ///     タップ入力を生成します。生成直後は無効です。
        /// </summary>
        /// <returns> 生成したタップ入力です。 </returns>
        public static MobileTapPopupConfirmInput Create()
        {
            GameObject inputObject = new GameObject(nameof(MobileTapPopupConfirmInput));
            return inputObject.AddComponent<MobileTapPopupConfirmInput>();
        }

        /// <summary>
        ///     タップの受け付けを開始します。
        /// </summary>
        public void Activate()
        {
            _isActive = true;
        }

        /// <summary>
        ///     タップの受け付けを終了します。
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
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
            OnTapped = null;
            Destroy(gameObject);
        }

        private readonly List<RaycastResult> _raycastResults = new();

        private PointerEventData _pointerEventData;
        private EventSystem _pointerEventSystem;
        private bool _isActive;

        /// <summary>
        ///     有効な間、画面のタップを通知する。
        /// </summary>
        private void Update()
        {
            // ポーズ中のメニュー操作でポップアップが閉じないよう、時間停止中は通知しない。
            if (!_isActive || Time.timeScale <= 0f)
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
                if (!touch.press.wasPressedThisFrame)
                {
                    continue;
                }

                // 仮想スティックやボタンなどの操作UIへのタップは、その操作に任せる。
                if (IsOverInteractiveUi(touch.position.ReadValue()))
                {
                    continue;
                }

                OnTapped?.Invoke();
                return;
            }
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
