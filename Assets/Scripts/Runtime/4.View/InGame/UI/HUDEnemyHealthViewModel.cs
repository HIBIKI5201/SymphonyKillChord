using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    using Camera = UnityEngine.Camera;

    /// <summary>
    ///     ロックオンHUDの表示値をViewへ反映する。
    /// </summary>
    public sealed class HUDEnemyHealthViewModel : IHUDEnemyHealthViewModel, IDisposable
    {
        /// <summary>
        ///     Viewを受け取り表示値の購読を構成する。
        /// </summary>
        /// <param name="view"> 表示を反映するView。 </param>
        public HUDEnemyHealthViewModel(HUDEnemyHealthView view)
        {
            _view = view;
            _mainCamera = Camera.main;

            _currentHealth
                .CombineLatest(_maxHealth, (current, max) => current / max)
                .Subscribe(ratio => _view.SetHealth(ratio))
                .RegisterTo(view.destroyCancellationToken);

            _displayState
                .Subscribe(_view.SetDisplayState)
                .RegisterTo(view.destroyCancellationToken);

            _uiPosition
                .Subscribe(_view.SetPosition)
                .RegisterTo(view.destroyCancellationToken);
        }

        /// <summary>
        ///     保持しているReactivePropertyを破棄する。
        /// </summary>
        public void Dispose()
        {
            _displayState.Dispose();
            _maxHealth.Dispose();
            _currentHealth.Dispose();
        }

        /// <summary>
        ///     DTOの値をロックオンHUDへ反映する。
        /// </summary>
        /// <param name="dto"> 反映する表示情報。 </param>
        public void Update(in HUDEnemyHealthDTO dto)
        {
            _maxHealth.Value = dto.MaxHealth;
            _currentHealth.Value = dto.CurrentHealth;
            _displayState.Value = dto.DisplayState;

            if (dto.DisplayState != LockOnDisplayState.Hidden && _mainCamera != null)
            {
                _uiPosition.Value = _mainCamera.WorldToScreenPoint(dto.TargetPosition);
            }
        }

        private readonly Camera _mainCamera;
        private readonly ReactiveProperty<Vector2> _uiPosition = new();
        private readonly ReactiveProperty<float> _maxHealth = new();
        private readonly ReactiveProperty<float> _currentHealth = new();
        private readonly ReactiveProperty<LockOnDisplayState> _displayState = new();
        private readonly HUDEnemyHealthView _view;
    }
}
