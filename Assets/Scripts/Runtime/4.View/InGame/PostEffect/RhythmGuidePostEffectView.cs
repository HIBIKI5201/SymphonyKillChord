using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.PostEffect
{
    public sealed class RhythmGuidePostEffectView : MonoBehaviour
    {
        public void SetColor(Color color)
        {
            _material.SetColor(COLOR, color);
        }
        public void OneShotRatio(Ease ease = Ease.InCirc, float duration = 0.1f)
        {
            _handle.TryComplete();
            _handle = LMotion.Create(1f, 0f, duration)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToMaterialFloat(_material, RATIO);
        }

        [Tooltip("フルスクリーン演出のMaterial。RendererFeatureに設定した物と同じアセットを指定")]
        [SerializeField] private Material _material;

        private void Awake()
        {
            _defaultRatio = _material.GetFloat(RATIO);
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
            _material.SetFloat(RATIO, _defaultRatio);
        }

        private float _defaultRatio;
        private MotionHandle _handle;
        private static readonly int COLOR = Shader.PropertyToID("_Color");
        private static readonly int RATIO = Shader.PropertyToID("_Alpha");
    }
}
