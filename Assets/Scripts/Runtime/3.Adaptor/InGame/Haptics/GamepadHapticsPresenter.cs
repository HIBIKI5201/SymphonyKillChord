using KillChord.Runtime.Adaptor.InGame.Battle;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Haptics
{
    /// <summary>
    ///     攻撃実行の通知を受け、ジャスト成立時にゲームパッド振動の再生をViewModelへ伝えるPresenter。
    /// </summary>
    public sealed class GamepadHapticsPresenter : IDisposable
    {
        /// <summary>
        ///     振動再生に必要な依存を受け取り、攻撃実行の通知へ購読する。
        /// </summary>
        /// <param name="attackSignal"> Presenterから攻撃成立の表示データを受け取るSignal。 </param>
        /// <param name="viewModel"> 振動再生指示の反映先。 </param>
        /// <param name="tutorialTargetBeatCountProvider"> 現在の色指定課題の対象拍数です。未指定や対象外では従来どおり振動します。 </param>
        public GamepadHapticsPresenter(
            IPlayerAttackSignal attackSignal,
            IGamepadHapticsViewModel viewModel,
            Func<int?> tutorialTargetBeatCountProvider = null)
        {
            _attackSignal = attackSignal ?? throw new ArgumentNullException(nameof(attackSignal));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _tutorialTargetBeatCountProvider = tutorialTargetBeatCountProvider;

            _attackSignal.OnAttackExecuted += AttackExecutedHandler;
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

            _attackSignal.OnAttackExecuted -= AttackExecutedHandler;
            _attackSignal = null;
        }

        private readonly IGamepadHapticsViewModel _viewModel;
        private readonly Func<int?> _tutorialTargetBeatCountProvider;
        private IPlayerAttackSignal _attackSignal;

        /// <summary>
        ///     ジャスト攻撃で振動し、色指定課題中は指定色との一致も必要とします。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した攻撃の拍種の整数値。 </param>
        /// <param name="isJustHit"> 攻撃・スキルに適用したジャスト成否。 </param>
        private void AttackExecutedHandler(int beatCount, bool isJustHit)
        {
            if (!isJustHit)
            {
                return;
            }

            int? targetBeatCount = _tutorialTargetBeatCountProvider?.Invoke();
            if (targetBeatCount.HasValue && beatCount != targetBeatCount.Value)
            {
                return;
            }

            _viewModel.PlayJustHitPulse();
        }
    }
}
