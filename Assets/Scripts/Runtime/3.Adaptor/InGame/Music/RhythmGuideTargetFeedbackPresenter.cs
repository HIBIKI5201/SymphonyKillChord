using KillChord.Runtime.Adaptor.InGame.Battle;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     攻撃成立通知を現在のチュートリアル対象拍と照合し、成功演出をViewModelへ伝えるPresenter。
    /// </summary>
    public sealed class RhythmGuideTargetFeedbackPresenter : IDisposable
    {
        /// <summary>
        ///     対象拍成功演出に必要な依存を受け取り、攻撃成立通知を購読する。
        /// </summary>
        /// <param name="attackSignal"> 攻撃成立の表示データを受け取るSignal。 </param>
        /// <param name="targetBeatCountProvider"> 現在のチュートリアル対象拍を取得する窓口。 </param>
        /// <param name="viewModel"> 成功演出の反映先。 </param>
        public RhythmGuideTargetFeedbackPresenter(
            IPlayerAttackSignal attackSignal,
            Func<int?> targetBeatCountProvider,
            IRhythmGuideTargetFeedbackViewModel viewModel)
        {
            _attackSignal = attackSignal ?? throw new ArgumentNullException(nameof(attackSignal));
            _targetBeatCountProvider = targetBeatCountProvider
                ?? throw new ArgumentNullException(nameof(targetBeatCountProvider));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            _attackSignal.OnAttackExecuted += AttackExecutedHandler;
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

            _attackSignal.OnAttackExecuted -= AttackExecutedHandler;
            _attackSignal = null;
        }

        private readonly Func<int?> _targetBeatCountProvider;
        private readonly IRhythmGuideTargetFeedbackViewModel _viewModel;
        private IPlayerAttackSignal _attackSignal;

        /// <summary>
        ///     現在の対象拍と一致する攻撃が成立した場合に成功演出を再生する。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した攻撃の拍種。 </param>
        /// <param name="_"> 攻撃に適用されたジャスト成否。この演出では使用しない。 </param>
        private void AttackExecutedHandler(int beatCount, bool _)
        {
            int? targetBeatCount = _targetBeatCountProvider.Invoke();
            if (!targetBeatCount.HasValue || beatCount != targetBeatCount.Value)
            {
                return;
            }

            _viewModel.PlayTargetBeatSuccessFeedback();
        }
    }
}
