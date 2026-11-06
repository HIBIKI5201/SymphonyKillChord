using KillChord.Runtime.Adaptor.InGame.Battle;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     攻撃成立通知のうちジャスト成立だけを拾い、リズムガイドの成立演出をViewModelへ伝えるPresenter。
    /// </summary>
    public sealed class RhythmGuideJustHitFeedbackPresenter : IDisposable
    {
        /// <summary>
        ///     ジャスト成立演出に必要な依存を受け取り、攻撃成立通知を購読する。
        /// </summary>
        /// <param name="attackSignal"> 攻撃成立の表示データを受け取るSignal。 </param>
        /// <param name="viewModel"> ジャスト成立演出の反映先。 </param>
        public RhythmGuideJustHitFeedbackPresenter(
            IPlayerAttackSignal attackSignal,
            IRhythmGuideJustHitFeedbackViewModel viewModel)
        {
            _attackSignal = attackSignal ?? throw new ArgumentNullException(nameof(attackSignal));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            _attackSignal.OnAttackExecuted += HandleAttackExecuted;
        }

        /// <summary>
        ///     攻撃成立通知の購読を解除する。
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

        private readonly IRhythmGuideJustHitFeedbackViewModel _viewModel;
        private IPlayerAttackSignal _attackSignal;

        /// <summary>
        ///     ジャストが成立した攻撃だけ成立演出を再生する。
        ///     ガイドのカーソル位置から再判定せず、攻撃に適用した判定結果をそのまま使う。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した攻撃の拍種の整数値。 </param>
        /// <param name="isJustHit"> 攻撃・スキルに適用したジャスト成否。 </param>
        private void HandleAttackExecuted(int beatCount, bool isJustHit)
        {
            if (!isJustHit)
            {
                return;
            }

            _viewModel.PlayJustHitFeedback(beatCount);
        }
    }
}
