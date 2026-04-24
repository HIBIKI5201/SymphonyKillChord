using KillChord.Runtime.Application.InGame.Enemy;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     砲兵が撃つ砲弾のコントローラー。
    /// </summary>
    public class ShellController : IDisposable
    {
        public ShellController(IShellViewModel viewModel, ShellReservationUsecase reservationUsecase, EnemyAIController enemyAIController)
        {
            _viewModel = viewModel;
            _reservationUsecase = reservationUsecase;
            _enemyAIController = enemyAIController;

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

        private void DealDamage()
        {
            _enemyAIController.DealProjectileDamage();
        }

        private void HandleReservedTimingReached()
        {
            if(FindDamageTarget())
            {
                DealDamage();
            }
            _viewModel.Detonate();
        }

        private readonly ShellReservationUsecase _reservationUsecase;
        private readonly EnemyAIController _enemyAIController;
        private IShellViewModel _viewModel;
    }
}
