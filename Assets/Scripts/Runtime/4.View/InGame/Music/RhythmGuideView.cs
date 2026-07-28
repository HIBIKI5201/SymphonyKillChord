using KillChord.Runtime.Adaptor.InGame.Music;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドの視覚的なレンダリングを行うViewクラス。
    /// </summary>
    public class RhythmGuideView : MonoBehaviour
    {
        /// <summary>
        ///     ビューモデルの状態に基づいてレンダリングを行う。
        /// </summary>
        /// <param name="viewModel"> リズムガイドビューモデル。 </param>
        public void Render(RhythmGuideViewModel viewModel)
        {
            RenderIndicator(viewModel.IndicatorNormalized);
            RenderAlpha(viewModel.HasTarget);
            RenderLabels(viewModel.Zones);
        }

        [Tooltip("インジケーターのルート。")]
        [SerializeField] private RectTransform _indicatorRoot;
        [Tooltip("リズム表示のCanvasGroup。")]
        [SerializeField] private CanvasGroup _rhythmCanvasGroup;

        [Tooltip("ラベルのプレハブ。")]
        [SerializeField] private RhythmGuideLabelView _labelPrefab;
        [Tooltip("ラベルの親要素。")]
        [SerializeField] private Transform _labelParent;
        [Tooltip("ラベルの配置半径。")]
        [SerializeField] private float _labelRadius;

        [Tooltip("開始角度。")]
        [SerializeField] private float _startAngle;
        [Tooltip("回転角度。")]
        [SerializeField] private float _sweepAngle;

        [Tooltip("ターゲット時の透明度。")]
        [SerializeField] private float _targetAlpha;
        [Tooltip("非ターゲット時の透明度。")]
        [SerializeField] private float _noTargetAlpha;

        private readonly List<RhythmGuideLabelView> _labelViews = new();

        /// <summary>
        ///     インジケーターの回転を更新する。
        /// </summary>
        /// <param name="normalized"> 正規化された位置。 </param>
        private void RenderIndicator(float normalized)
        {
            if (_indicatorRoot == null) return;

            float angle = _startAngle + normalized * _sweepAngle;
            _indicatorRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>
        ///     CanvasGroupの透明度を更新する。
        /// </summary>
        /// <param name="hasTarget"> ターゲットの有無。 </param>
        private void RenderAlpha(bool hasTarget)
        {
            if (_rhythmCanvasGroup == null) return;

            _rhythmCanvasGroup.alpha = hasTarget ? _targetAlpha : _noTargetAlpha;
        }

        /// <summary>
        ///     ラベルの表示を更新する。
        /// </summary>
        /// <param name="zones"> 判定ゾーンのリスト。 </param>
        private void RenderLabels(IReadOnlyList<RhythmGuideZoneDto> zones)
        {
            if (zones == null) return;

            EnsureLabelCount(zones.Count);

            for (int i = 0; i < zones.Count; i++)
            {
                RhythmGuideZoneDto zone = zones[i];

                float centerNormalized = (zone.StartNormalized + zone.EndNormalized) * 0.5f;

                _labelViews[i].Render(
                    zone.BeatCount,
                    centerNormalized,
                    _labelRadius,
                    _startAngle,
                    _sweepAngle
                );
            }
        }

        /// <summary>
        ///     ラベルViewの数を調整する。
        /// </summary>
        /// <param name="count"> 必要なラベル数。 </param>
        private void EnsureLabelCount(int count)
        {
            while (_labelViews.Count < count)
            {
                RhythmGuideLabelView view = Instantiate(_labelPrefab, _labelParent);
                _labelViews.Add(view);
            }

            for (int i = 0; i < _labelViews.Count; i++)
            {
                _labelViews[i].gameObject.SetActive(i < count);
            }
        }
    }
}
