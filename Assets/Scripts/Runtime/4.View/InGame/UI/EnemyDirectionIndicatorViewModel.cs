using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     敵方向表示の希望状態を保持し、描画Viewへ反映する。
    /// </summary>
    public sealed class EnemyDirectionIndicatorViewModel : IEnemyDirectionIndicatorViewModel, IDisposable
    {
        /// <summary>
        ///     反映先のViewを受け取る。
        /// </summary>
        /// <param name="view"> 敵方向表示View。 </param>
        public EnemyDirectionIndicatorViewModel(EnemyDirectionIndicatorView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (view.Capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(view), "表示スロット数は1以上である必要があります。");
            }

            _directions = new ReactiveProperty<Vector3>[view.Capacity];
            _visibilityStates = new ReactiveProperty<bool>[view.Capacity];

            for (int i = 0; i < view.Capacity; i++)
            {
                int slotIndex = i;
                ReactiveProperty<Vector3> direction = new();
                ReactiveProperty<bool> visibilityState = new();

                direction
                    .Subscribe(value => view.SetDirection(slotIndex, value))
                    .RegisterTo(view.destroyCancellationToken);
                visibilityState
                    .Subscribe(value => view.SetVisibility(slotIndex, value))
                    .RegisterTo(view.destroyCancellationToken);

                _directions[i] = direction;
                _visibilityStates[i] = visibilityState;
            }
        }

        /// <summary> 使用可能な表示スロット数。 </summary>
        public int Capacity => _visibilityStates.Length;

        /// <summary>
        ///     1スロット分の表示情報をViewへ反映する。
        /// </summary>
        /// <param name="dto"> 反映する表示情報。 </param>
        public void Update(in EnemyDirectionIndicatorDTO dto)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(EnemyDirectionIndicatorViewModel));
            }

            if (dto.SlotIndex < 0 || dto.SlotIndex >= _visibilityStates.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dto), dto.SlotIndex, "表示スロット番号が範囲外です。");
            }

            if (dto.IsVisible)
            {
                _directions[dto.SlotIndex].Value = dto.Direction;
            }

            _visibilityStates[dto.SlotIndex].Value = dto.IsVisible;
        }

        /// <summary>
        ///     保持しているViewと表示状態を解放する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            for (int i = 0; i < _visibilityStates.Length; i++)
            {
                _directions[i].Dispose();
                _visibilityStates[i].Dispose();
            }
        }

        private readonly ReactiveProperty<Vector3>[] _directions;
        private readonly ReactiveProperty<bool>[] _visibilityStates;
        private bool _isDisposed;
    }
}
