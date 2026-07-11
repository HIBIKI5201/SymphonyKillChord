using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    using Camera = UnityEngine.Camera;
    public sealed class HUDEnemyHealthViewModel : IHUDEnemyHealthViewModel, IDisposable
    {
        public HUDEnemyHealthViewModel(HUDEnemyHealthView view)
        {
            _view = view;
            mainCamera = Camera.main;

            _currentHealth
                .CombineLatest(_maxHealth, (current, max) => current / max)
                .Subscribe(ratio => _view.SetHealth(ratio))
                .RegisterTo(view.destroyCancellationToken);

            _isLockon
                .Subscribe(_view.SetLockonEnable)
                .RegisterTo(view.destroyCancellationToken);

            _uiPosition
                .Subscribe(_view.SetPosition)
                .RegisterTo(view.destroyCancellationToken);
        }

        public void Dispose()
        {
            _isLockon.Dispose();
            _maxHealth.Dispose();
            _currentHealth.Dispose();
        }

        public void Update(in HUDEnemyHealthDTO dto)
        {
            _maxHealth.Value = dto.MaxHealth;
            _currentHealth.Value = dto.CurrentHealth;
            _isLockon.Value = dto.IsLockon;
            _uiPosition.Value = mainCamera.WorldToScreenPoint(dto.TargetPosition);
        }

        private readonly Camera mainCamera;
        private readonly ReactiveProperty<Vector2> _uiPosition = new();
        private readonly ReactiveProperty<float> _maxHealth = new();
        private readonly ReactiveProperty<float> _currentHealth = new();
        private readonly ReactiveProperty<bool> _isLockon = new();
        private readonly HUDEnemyHealthView _view;
    }
}
