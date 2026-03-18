using LitMotion;
using UnityEngine;

namespace DevelopProducts.RenderingExtension
{
    public sealed class TestFade : MonoBehaviour
    {
        [SerializeField] private BitonicPixelSortingFeatureMod _renderFeauture;
        [SerializeField] private GameObject _sample1;
        [SerializeField] private GameObject _sample2;

        [Space]
        [SerializeField] private float _duration;

        private bool _toggle;

        private MotionHandle _handle;
        private void OnDestroy()
        {
            _handle.TryCancel();
        }
        [ContextMenu("Play")]
        void Animation()
            => Animation(_duration);
        void Animation(float duration)
        {
            _handle.TryComplete();
            MotionSequenceBuilder builder = LSequence.Create();

            SetTrueFeauture();

            builder
                .Append(LMotion.Create(0f, 1f, duration / 2)
                    .WithDelay(0.5f)
                    .WithOnComplete(ToggleActive)
                    .Bind(_renderFeauture, (value, state) => state.thresholdMax = value))
                .Append(LMotion.Create(0f, 1f, duration / 2)
                    .Bind(_renderFeauture, (value, state) => state.thresholdMin = value));
            _handle = builder.Run(x => x.WithOnComplete(SetFalseFeauture));
            _handle.Time = 0;
        }

        private void ToggleActive()
        {
            _toggle = !_toggle;
            _sample1.SetActive(_toggle);
            _sample2.SetActive(!_toggle);
        }
        private void SetFalseFeauture()
            => SetActiveFeauture(false);
        private void SetTrueFeauture()
            => SetActiveFeauture(true);
        private void SetActiveFeauture(bool enabled)
        {
            _renderFeauture.SetActive(enabled);
        }
    }
}
