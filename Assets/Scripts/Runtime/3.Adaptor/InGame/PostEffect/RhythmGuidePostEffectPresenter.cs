using KillChord.Runtime.Adaptor.InGame.Battle;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.PostEffect
{
    /// <summary>
    ///     攻撃実行の通知を受け、ジャスト成否とビート色を全画面演出のViewModelへ伝えるPresenter。
    /// </summary>
    public sealed class RhythmGuidePostEffectPresenter : IDisposable
    {
        /// <summary>
        ///     全画面演出に必要な依存を受け取り、攻撃実行の通知へ購読する。
        /// </summary>
        /// <param name="attackSignal"> Presenterから攻撃成立の表示データを受け取るSignal。 </param>
        /// <param name="beatViewModel"> 拍種ごとのビート色の取得元。 </param>
        /// <param name="viewModel"> 表示データの反映先。 </param>
        public RhythmGuidePostEffectPresenter(
            IPlayerAttackSignal attackSignal,
            IRhythmGuideBeatViewModel beatViewModel,
            IRhythmGuidePostEffectViewModel viewModel)
        {
            _attackSignal = attackSignal ?? throw new ArgumentNullException(nameof(attackSignal));
            _beatViewModel = beatViewModel ?? throw new ArgumentNullException(nameof(beatViewModel));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

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

        private readonly IRhythmGuideBeatViewModel _beatViewModel;
        private readonly IRhythmGuidePostEffectViewModel _viewModel;
        private IPlayerAttackSignal _attackSignal;

        /// <summary>
        ///     攻撃入力時にジャスト成否とビート色をViewModelへ送る。
        /// </summary>
        /// <param name="beatCount"> 入力時に確定した攻撃の拍種の整数値。 </param>
        /// <param name="isJustHit"> 攻撃・スキルに適用したジャスト成否。 </param>
        private void AttackExecutedHandler(int beatCount, bool isJustHit)
        {
            // ビート色を取得できない場合はガイドが未構築のため、演出を出さない。
            if (!_beatViewModel.TryGetBeatColor(beatCount, out Color color))
            {
                return;
            }

            // 履歴更新後の時刻や描画済みカーソルから再判定せず、入力結果をそのまま表示する。
            _viewModel.Play(new RhythmGuidePostEffectDto(isJustHit, color));
        }
    }
}
