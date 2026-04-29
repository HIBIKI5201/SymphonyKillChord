using KillChord.Runtime.Adaptor.InGame.Music;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public class RhythmGuideView : MonoBehaviour
    {
        public void Render(RhythmGuideViewModel viewModel)
        {
            RenderIndicator(viewModel.IndicatorNormalized);
            RenderAlpha(viewModel.HasTarget);
            RenderLabels(viewModel.Zones);
        }

        [SerializeField] private RectTransform _indicatorRoot;
        [SerializeField] private CanvasGroup _rhythmCanvasGroup;

        [SerializeField] private RhythmGuideLabelView _labelPrefab;
        [SerializeField] private Transform _labelParent;
        [SerializeField] private float _labelRadius;

        [SerializeField] private float _startAngle;
        [SerializeField] private float _sweepAngle;

        [SerializeField] private float _targetAlpha;
        [SerializeField] private float _noTargetAlpha;

        private readonly List<RhythmGuideLabelView> _labelViews = new();

        private void RenderIndicator(float normalized)
        {
            if (_indicatorRoot == null) return;

            float angle = _startAngle + normalized * _sweepAngle;
            _indicatorRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void RenderAlpha(bool hasTarget)
        {
            if (_rhythmCanvasGroup == null) return;

            _rhythmCanvasGroup.alpha = hasTarget ? _targetAlpha : _noTargetAlpha;
        }

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
