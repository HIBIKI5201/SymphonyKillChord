using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Utility.Collections;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ロックオン候補または確定対象のHUDを表示する。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.CAMERA_FOLLOW)]
    public sealed class HUDEnemyHealthView : MonoBehaviour
    {
        /// <summary> HUD位置の更新タイミングを通知する。 </summary>
        public event Action OnUpdate;

        /// <summary>
        ///     HUDの表示状態を切り替える。
        /// </summary>
        /// <param name="displayState"> 適用する表示状態。 </param>
        public void SetDisplayState(LockOnDisplayState displayState)
        {
            _shakeHandle.TryComplete();

            switch (displayState)
            {
                case LockOnDisplayState.Candidate:
                    // ロックオン表示のモーションが残っているとアルファを上書きされるため、先に停止する。
                    _visibleHandle.TryCancel();
                    _healthImage.enabled = true;
                    _healthImage.sprite = null;
                    _reticleAlpha.alpha = 0f;
                    _healthImage.color = _candidateColor;
                    _healthImage.rectTransform.sizeDelta = _candidateSize;
                    break;
                case LockOnDisplayState.LockedOn:
                    _healthImage.enabled = false;
                    MotionVisibleLockedOn();
                    break;
                default:
                    // ロックオン表示のモーションが残っているとアルファを上書きされるため、先に停止する。
                    _visibleHandle.TryCancel();
                    _reticleAlpha.alpha = 0f;
                    _healthImage.enabled = false;
                    break;
            }
        }

        /// <summary>
        ///     確定対象の体力表示を更新する。
        /// </summary>
        /// <param name="ratio"> 最大体力に対する現在体力の比率。 </param>
        public void SetHealth(float ratio)
        {
            if (float.IsNaN(ratio))
                return;
            _barHandle.TryCancel();
            _barHandle = LMotion.Create(_healthBarImage.fillAmount, Mathf.Clamp01(ratio), 0.1f)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToFillAmount(_healthBarImage);
        }

        /// <summary>
        ///     HUDのスクリーン座標を更新する。
        /// </summary>
        /// <param name="position"> 表示するスクリーン座標。 </param>
        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
        private void Awake()
        {
            _lockedOnSize = _reticleRectTransform.sizeDelta;
        }
        private void LateUpdate()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
            _shakeHandle.TryCancel();
            _visibleHandle.TryCancel();
            _barHandle.TryCancel();
        }
        private void MotionVisibleLockedOn()
        {
            _barHandle.TryComplete();
            _visibleHandle.TryComplete();
            _visibleHandle = LSequence.Create()
                .Join(LMotion.Create(_lockedOnSize * 3f, _lockedOnSize, 0.1f)
                    .WithEase(Ease.Linear)
                    .BindToSizeDelta(_reticleRectTransform))
                .Join(LMotion.Create(0f, 1f, 0.1f)
                    .WithEase(Ease.Linear)
                    .BindToAlpha(_reticleAlpha))
                .Run();
        }
        [SerializeField]
        private RectTransform _reticleRectTransform;

        [SerializeField]
        private CanvasGroup _reticleAlpha;
        [SerializeField]
        private Image _healthBarImage;

        [SerializeField, Tooltip("ロックオン候補と確定対象の表示に使用するImage。")]
        private Image _healthImage;

        [SerializeField, Tooltip("候補表示に使用する点のサイズ。")]
        private Vector2 _candidateSize = new Vector2(12f, 12f);

        [SerializeField, Tooltip("候補表示に使用する点の色。")]
        private Color _candidateColor = new Color(1f, 0.85f, 0.2f, 1f);

        private MotionHandle _shakeHandle;
        private MotionHandle _visibleHandle;
        private MotionHandle _barHandle;
        private Vector2 _lockedOnSize;
    }
}
