using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Haptics
{
    /// <summary>
    ///     プレイヤーの被弾通知を受け、ゲームパッド振動の再生をViewModelへ伝えるPresenter。
    /// </summary>
    public sealed class PlayerDamageHapticsPresenter : IDisposable
    {
        /// <summary>
        ///     振動再生に必要な依存を受け取り、プレイヤーの被弾通知へ購読する。
        /// </summary>
        /// <param name="playerEntity"> 被弾を通知するプレイヤーのEntity。 </param>
        /// <param name="viewModel"> 振動再生指示の反映先。 </param>
        public PlayerDamageHapticsPresenter(CharacterEntity playerEntity, IGamepadHapticsViewModel viewModel)
        {
            _playerEntity = playerEntity ?? throw new ArgumentNullException(nameof(playerEntity));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            _playerEntity.OnDamageTaken += DamageTakenHandler;
        }

        /// <summary>
        ///     被弾通知の購読を解除する。
        /// </summary>
        public void Dispose()
        {
            if (_playerEntity == null)
            {
                return;
            }

            _playerEntity.OnDamageTaken -= DamageTakenHandler;
            _playerEntity = null;
        }

        private readonly IGamepadHapticsViewModel _viewModel;
        private CharacterEntity _playerEntity;

        /// <summary>
        ///     被弾したときに振動を再生する。
        /// </summary>
        /// <param name="damage"> 受けたダメージ。 </param>
        private void DamageTakenHandler(Damage damage)
        {
            _viewModel.PlayDamagePulse();
        }
    }
}
