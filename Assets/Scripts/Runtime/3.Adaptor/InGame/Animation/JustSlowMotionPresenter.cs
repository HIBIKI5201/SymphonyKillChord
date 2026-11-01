using KillChord.Runtime.Adaptor.InGame.Battle;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     攻撃実行の通知を受け、ジャスト成立時にアニメーションのスローモーションをViewModelへ伝えるPresenter。
    /// </summary>
    public sealed class JustSlowMotionPresenter : IDisposable
    {
        /// <summary>
        ///     スローモーション再生に必要な依存を受け取り、攻撃実行の通知へ購読する。
        /// </summary>
        /// <param name="attackSignal"> Presenterから攻撃成立の表示データを受け取るSignal。 </param>
        /// <param name="viewModel"> スローモーション再生指示の反映先。 </param>
        public JustSlowMotionPresenter(
            IPlayerAttackSignal attackSignal,
            IJustSlowMotionViewModel viewModel)
        {
            _attackSignal = attackSignal ?? throw new ArgumentNullException(nameof(attackSignal));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            _attackSignal.OnAttackExecuted += HandleAttackExecuted;
        }

        /// <summary>
        ///     攻撃実行通知の購読を解除する。
        /// </summary>
        public void Dispose()
        {
            if (_attackSignal == null)
            {
                return;
            }

            _attackSignal.OnAttackExecuted -= HandleAttackExecuted;
            _attackSignal = null;
        }

        private readonly IJustSlowMotionViewModel _viewModel;
        private IPlayerAttackSignal _attackSignal;

        /// <summary>
        ///     ジャスト攻撃のときだけスローモーションを再生する。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した攻撃の拍種の整数値。 </param>
        /// <param name="isJustHit"> 攻撃・スキルに適用したジャスト成否。 </param>
        private void HandleAttackExecuted(int beatCount, bool isJustHit)
        {
            if (!isJustHit)
            {
                return;
            }

            _viewModel.PlayJustSlowMotion();
        }
    }
}
