using LitMotion;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    public sealed class HUDEnemyHealthView : MonoBehaviour
    {
        public event Action OnUpdate;
        public void SetLockonEnable(bool isLockon)
        {
            _handle.TryComplete();
            _healthImage.enabled = isLockon;
        }

        public void SetHealth(float ratio)
        {
            if (float.IsNaN(ratio))
                return;

            _handle.TryCancel();
            _handle = LMotion.Create(_ratio, Mathf.Clamp01(ratio), 0.1f)
                .WithEase(Ease.InOutCirc)
                .Bind(this, static (value, mat) => mat.SetValue(value));
        }
        private void Awake()
        {
            _material = _healthImage.material;
        }
        private void Update()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
            _handle.TryCancel();
        }
        private void SetValue(float ratio)
        {
            _ratio = Mathf.Clamp01(ratio);
            _material.SetFloat(_shaderPropertyId, 1f - _ratio);
        }
        [SerializeField] private Image _healthImage;

        private float _ratio;
        private Material _material;
        private static int _shaderPropertyId = Shader.PropertyToID("_Ratio");
        private MotionHandle _handle;
    }
}
