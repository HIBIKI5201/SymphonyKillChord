using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲兵が撃つ砲弾のコントローラー。
    /// </summary>
    public class ShellController : IDisposable
    {
        public ShellController(ShellEntity entity, IShellView viewModel, ShellReservationUsecase reservationUsecase, IAttacker attacker, IDefender defender, ShellAttackUsecase attackUsecase)
        {
            _entity = entity;
            _viewModel = viewModel;
            _reservationUsecase = reservationUsecase;
            _attacker = attacker;
            _defender = defender;
            _attackUsecase = attackUsecase;

        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        /// <param name="enemyBattleState"></param>
        public void Activate(EnemyBattleState enemyBattleState)
        {
            _attacker = enemyBattleState.Attacker;
            _defender = enemyBattleState.Target;
            _entity.Reset(enemyBattleState.CurrentAttack);
            _reservationUsecase.OnReservedTimingReached += HandleReservedTimingReached;
            _reservationUsecase.On2BeatBefore += Handle2BeatBefore;
            _reservationUsecase.On1BeatBefore += Handle1BeatBefore;
            _reservationUsecase.ReserveDetonate();
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            _reservationUsecase.Cancel();
            _reservationUsecase.OnReservedTimingReached -= HandleReservedTimingReached;
            _reservationUsecase.On2BeatBefore -= Handle2BeatBefore;
            _reservationUsecase.On1BeatBefore -= Handle1BeatBefore;
            _attacker = null;
            _defender = null;
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
            bool rst = _viewModel.FindDamageTarget();
            return rst;
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

        private void Handle2BeatBefore()
        {
            Debug.Log("[ShellController] 爆発の2拍前");
        }

        private void Handle1BeatBefore()
        {
            Debug.Log("[ShellController] 爆発の1拍前");
        }

        private readonly ShellEntity _entity;
        private readonly ShellReservationUsecase _reservationUsecase;
        private readonly IShellView _viewModel;
        private IAttacker _attacker;
        private IDefender _defender;
        private readonly ShellAttackUsecase _attackUsecase;
    }
}
