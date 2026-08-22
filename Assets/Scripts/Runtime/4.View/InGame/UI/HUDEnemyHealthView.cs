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
            _displayState = displayState;

            switch (displayState)
            {
                case LockOnDisplayState.Candidate:
                    _healthImage.enabled = true;
                    _healthImage.sprite = null;
                    _healthImage.color = _candidateColor;
                    _healthImage.rectTransform.sizeDelta = _candidateSize;
                    break;
                case LockOnDisplayState.LockedOn:
                    _healthImage.enabled = true;
                    _healthImage.color = _lockedOnColor;
                    MotionVisibleLockedOn();
                    _healthImage.sprite = _sprites[_index];
                    break;
                default:
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
            int index = RatioToIndex(ratio);
            if (_index != index)
            {
                _index = index;

                if (_displayState != LockOnDisplayState.LockedOn)
                {
                    return;
                }

                _healthImage.sprite = _sprites[_index];

                _shakeHandle.TryCancel();
                _shakeHandle = LSequence.Create()
                    .Join(LMotion.Punch.Create(0f, 30f, 0.3f)
                        .WithEase(Ease.OutCirc)
                        .WithFrequency(Random.Range(6, 10))
                        .BindToAnchoredPositionY(_healthImage.rectTransform))
                    .Join(LMotion.Punch.Create(0f, 30f, 0.3f)
                        .WithEase(Ease.OutCirc)
                        .WithFrequency(Random.Range(6, 10))
                        .BindToAnchoredPositionX(_healthImage.rectTransform))
                    .Run(x => x.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale));
            }
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
            _lockedOnSize = _healthImage.rectTransform.sizeDelta;
            _lockedOnColor = _healthImage.color;
            _index = _sprites.Length - 1;
            _healthImage.sprite = _sprites[^1];
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
        }
        private int RatioToIndex(float ratio)
        {
            return Mathf.Clamp(Mathf.RoundToInt(ratio * _sprites.Length), 0, _sprites.Length - 1);
        }
        private void MotionVisibleLockedOn()
        {
            _visibleHandle.TryComplete();
            _visibleHandle = LSequence.Create()
                .Join(LMotion.Create(_lockedOnSize * 3f, _lockedOnSize, 0.1f)
                    .WithEase(Ease.Linear)
                    .BindToSizeDelta(_healthImage.rectTransform))
                .Join(LMotion.Create(0f, _lockedOnColor.a, 0.1f)
                    .WithEase(Ease.Linear)
                    .BindToColorA(_healthImage))
                .Run();
        }

        [SerializeField, Tooltip("ロックオン候補と確定対象の表示に使用するImage。")]
        private Image _healthImage;

        [SerializeField, Tooltip("確定ロックオン時の体力割合別スプライト。")]
        private Sprite[] _sprites;

        [SerializeField, Tooltip("候補表示に使用する点のサイズ。")]
        private Vector2 _candidateSize = new Vector2(12f, 12f);

        [SerializeField, Tooltip("候補表示に使用する点の色。")]
        private Color _candidateColor = new Color(1f, 0.85f, 0.2f, 1f);

        private int _index;
        private MotionHandle _shakeHandle;
        private MotionHandle _visibleHandle;
        private LockOnDisplayState _displayState;
        private Vector2 _lockedOnSize;
        private Color _lockedOnColor;
    }
}
