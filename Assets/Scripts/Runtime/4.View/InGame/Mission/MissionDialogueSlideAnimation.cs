using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     会話UI演出を管理するクラス。
    /// </summary>
    public sealed class MissionDialogueSlideAnimation : MissionDialogueAnimationBase
    {
        /// <inheritdoc />
        public override void HideImmediate()
        {
            _motion.TryCancel();
            if (_isInitialized && (_panel == null || _canvasGroup == null))
            {
                return;
            }

            Initialize();
            _panel.anchoredPosition = GetHiddenPosition();
            _canvasGroup.alpha = 0f;
        }

        /// <inheritdoc />
        public override void Show()
        {
            Initialize();
            _motion.TryCancel();
            _panel.anchoredPosition = GetHiddenPosition();
            _canvasGroup.alpha = 1f;
            MoveTo(_shownPosition, _showDuration, _showEase, null);
        }

        /// <inheritdoc />
        public override void Hide(Action onCompleted)
        {
            Initialize();
            MoveTo(GetHiddenPosition(), _hideDuration, _hideEase, () =>
            {
                _canvasGroup.alpha = 0f;
                onCompleted?.Invoke();
            });
        }

        /// <inheritdoc />
        public override void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            if (_motion.IsActive())
            {
                _motion.PlaybackSpeed = isPaused ? 0f : 1f;
            }
        }

        [SerializeField, Tooltip("会話UIの演出が行われるパネル")]
        private RectTransform _panel;
        [SerializeField, Tooltip("会話UIのCanvasGroup")]
        private CanvasGroup _canvasGroup;
        [SerializeField, Min(0f), Tooltip("入場演出の秒数")]
        private float _showDuration = 0.25f;
        [SerializeField, Min(0f), Tooltip("退場演出の秒数")]
        private float _hideDuration = 0.2f;
        [SerializeField, Tooltip("入場演出のEasing")]
        private Ease _showEase = Ease.OutCubic;
        [SerializeField, Tooltip("退場演出のEasing")]
        private Ease _hideEase = Ease.InCubic;
        [SerializeField, Min(0f), Tooltip("画面外へ退避する際の余白")]
        private float _offscreenPadding = 16f;

        private readonly Vector3[] _corners = new Vector3[4];
        private MotionHandle _motion;
        private Vector2 _shownPosition;
        private bool _isInitialized;
        private bool _isPaused;

        /// <summary>
        ///     再生中のスライドアニメーションを止める。
        /// </summary>
        private void OnDisable()
        {
            _motion.TryCancel();
        }

        /// <summary>
        ///     初期化処理。
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            _shownPosition = _panel.anchoredPosition;
            _isInitialized = true;
        }

        /// <summary>
        ///     パネル全体が親Rectの右端に出る位置を算出する。
        /// </summary>
        private Vector2 GetHiddenPosition()
        {
            RectTransform parent = (RectTransform)_panel.parent;
            Vector2 currentPosition = _panel.anchoredPosition;
            _panel.anchoredPosition = _shownPosition;
            _panel.GetWorldCorners(_corners);
            float left = float.PositiveInfinity;
            for (int i = 0; i < _corners.Length; i++)
            {
                left = Mathf.Min(left, parent.InverseTransformPoint(_corners[i]).x);
            }
            _panel.anchoredPosition = currentPosition;
            return _shownPosition + Vector2.right * Mathf.Max(0f, parent.rect.xMax - left + _offscreenPadding);
        }

        /// <summary>
        ///     会話UIを指定位置に移動する。
        /// </summary>
        private void MoveTo(Vector2 destination, float duration, Ease ease, Action onCompleted)
        {
            _motion.TryCancel();
            if (duration <= 0f)
            {
                _panel.anchoredPosition = destination;
                onCompleted?.Invoke();
                return;
            }
            _motion = LMotion.Create(_panel.anchoredPosition, destination, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => onCompleted?.Invoke())
                .BindToAnchoredPosition(_panel)
                .AddTo(gameObject);
            SetPaused(_isPaused);
        }
    }
}
