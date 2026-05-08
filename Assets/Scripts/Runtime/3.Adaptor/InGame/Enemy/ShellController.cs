using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Enemy;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲兵が撃つ砲弾のコントローラー。
    /// </summary>
    public class ShellController : IDisposable
    {
        public ShellController(ShellEntity entity, IShellViewModel viewModel, ShellReservationUsecase reservationUsecase, IAttacker attacker, IDefender defender, ShellAttackUsecase attackUsecase)
        {
            _entity = entity;
            _viewModel = viewModel;
            _reservationUsecase = reservationUsecase;
            _attacker = attacker;
            _defender = defender;
            _attackUsecase = attackUsecase;

            _reservationUsecase.OnReservedTimingReached += HandleReservedTimingReached;
            _reservationUsecase.ReserveDetonate();
        }

        public void Dispose()
        {
            _reservationUsecase.Dispose();
            _reservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
        }

        /// <summary>
        ///     爆発時プレイヤーに命中したか確認する。
        /// </summary>
        /// <returns></returns>
        private bool FindDamageTarget()
        {
            return _viewModel.FindDamageTarget();
        }

        /// <summary>
        ///     ダメージを与える処理。
        /// </summary>
        private void DealDamage()
        {
            _attackUsecase.ExecuteAttack(_entity.AttackDefinition, _attacker, _defender);
        }

        /// <summary>
        ///     予約タイミングが到達した時の処理。
        /// </summary>
        private void HandleReservedTimingReached()
        {
            if(FindDamageTarget())
            {
                DealDamage();
            }
            _viewModel.Detonate();
        }

        private readonly ShellEntity _entity;
        private readonly ShellReservationUsecase _reservationUsecase;
        private readonly IShellViewModel _viewModel;
        private readonly IAttacker _attacker;
        private readonly IDefender _defender;
        private readonly ShellAttackUsecase _attackUsecase;
    }
}
