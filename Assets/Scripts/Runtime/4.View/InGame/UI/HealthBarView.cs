using KillChord.Runtime.Adaptor.InGame.UI;
using LitMotion;
using LitMotion.Extensions;
using R3;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.UI
{
    public sealed class HealthBarView : MonoBehaviour
    {
        public void Bind(IHealthHudViewModel vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm), "HPテキストのViewModelがNULL。");
            vm.HealthHudDTO
                .Subscribe(UpdateHealthBar)
                .RegisterTo(destroyCancellationToken);
        }


        [SerializeField, Tooltip("現在HPを表示するImage")]
        private Image _healthBarImage;

        private MotionHandle _handle;

        private void Awake()
        {
            if(_healthBarImage == null)
            {
                Debug.LogError($"[{nameof(HealthBarView)}] のフィールドがNullです", this);
            }
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        private void UpdateHealthBar(HealthHudDTO dto)
        {
            // 参照欠落時は更新しない
            if (_healthBarImage == null) return;

            float newFillAmount = dto.CurrentHealth / dto.MaxHealth;
            float currentFillAmount = _healthBarImage.fillAmount;

            if (currentFillAmount == newFillAmount)
            {
                return;
            }

            _handle.TryComplete();
            if (newFillAmount <= currentFillAmount)
            {
                _handle = LSequence.Create()
                    .Join(LMotion.Create(currentFillAmount,newFillAmount,0.2f)
                        .BindToFillAmount(_healthBarImage))
                    .Join(LMotion.Create(Color.red,Color.white ,0.05f)
                        .WithLoops(6)
                        .BindToColor(_healthBarImage))
                    .Run();
            }
            else
            {
                _handle = LSequence.Create()
                    .Join(LMotion.Create(currentFillAmount, newFillAmount, 0.2f)
                        .BindToFillAmount(_healthBarImage))
                    .Join(LMotion.Create(Color.green, Color.white, 3f)
                        .BindToColor(_healthBarImage))
                    .Run();
            }
        }
    }
}
