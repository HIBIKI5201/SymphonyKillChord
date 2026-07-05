using LitMotion;
using LitMotion.Extensions;
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
            _handle = LMotion.Create(_healthImage.fillAmount, Mathf.Clamp01(ratio), 0.1f)
                .WithEase(Ease.InOutCirc)
                .BindToFillAmount(_healthImage);
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

        [SerializeField] private Image _healthImage;
        private MotionHandle _handle;
    }
}
