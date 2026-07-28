using LitMotion;
using LitMotion.Extensions;
using R3;
using UnityEngine;

namespace KillChord.Runtime.View.OutGame.Scenario
{
    /// <summary>
    ///     シナリオ操作バーの開閉演出を制御するViewクラス。
    /// </summary>
    public class CommandBarToggleView : MonoBehaviour
    {
        /// <summary>
        ///     操作バーの開閉状態を切り替える。
        /// </summary>
        public void Toggle()
        {
            SetExpanded(!_isExpanded);
        }

        [SerializeField, Tooltip("開閉演出を行う操作バーのRectTransform。")]
        private RectTransform _commandBar;

        [SerializeField, Tooltip("開閉対象となるボタン群のCanvasGroup。")]
        private CanvasGroup _buttonGroupCanvas;

        [SerializeField, Min(0f), Tooltip("展開時の操作バー横幅。")]
        private float _expandedWidth;

        [SerializeField, Min(0f), Tooltip("折り畳み時の操作バー横幅。")]
        private float _callapsedWidth;

        [SerializeField, Tooltip("開閉演出にかかる時間。")]
        private float _duration;

        [SerializeField, Tooltip("展開時のイージング。")]
        private Ease _openEase;

        [SerializeField, Tooltip("折り畳み時のイージング。")]
        private Ease _closeEase;

        private bool _isExpanded = true;
        private MotionHandle _sizeMotionHandle;
        private MotionHandle _fadeMotionHandle;

        /// <summary>
        ///     操作バーの開閉状態を設定します。
        /// </summary>
        /// <param name="isExpanded"> 開く場合はtrue。 </param>
        private void SetExpanded(bool isExpanded)
        {
            _isExpanded = isExpanded;

            if (_sizeMotionHandle.IsActive())
            {
                _sizeMotionHandle.Cancel();
            }

            if (_fadeMotionHandle.IsActive())
            {
                _fadeMotionHandle.Cancel();
            }

            float targetWidth = isExpanded ? _expandedWidth : _callapsedWidth;
            Ease targetEase = isExpanded ? _openEase : _closeEase;

            if (isExpanded)
            {
                // ボタン群を表示状態にする。
                _buttonGroupCanvas.gameObject.SetActive(true);
                _buttonGroupCanvas.interactable = true;
                _buttonGroupCanvas.blocksRaycasts = true;
            }
            else
            {
                // 閉じる状態でクリックできないようにする。
                _buttonGroupCanvas.interactable = false;
                _buttonGroupCanvas.blocksRaycasts = false;
            }

            Vector2 startSize = _commandBar.sizeDelta;
            Vector2 targetSize = new Vector2(targetWidth, startSize.y);

            // 操作バーのサイズ変更演出。
            _sizeMotionHandle = LMotion
                .Create(startSize, targetSize, _duration)
                .WithEase(targetEase)
                .BindToSizeDelta(_commandBar)
                .AddTo(gameObject);

            // ボタン群のフェード演出。
            _fadeMotionHandle = LMotion
                .Create(_buttonGroupCanvas.alpha, isExpanded ? 1f : 0f, _duration)
                .WithEase(targetEase)
                .WithOnComplete(() =>
                {
                    if (!isExpanded)
                    {
                        _buttonGroupCanvas.gameObject.SetActive(false);
                    }
                })
                .BindToAlpha(_buttonGroupCanvas)
                .AddTo(gameObject);
        }
    }
}
