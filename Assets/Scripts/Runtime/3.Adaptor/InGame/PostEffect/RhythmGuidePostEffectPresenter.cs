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
        /// <param name="playerAttackController"> 攻撃実行の通知元。 </param>
        /// <param name="beatViewModel"> ジャスト成否とビート色の取得元。 </param>
        /// <param name="viewModel"> 表示データの反映先。 </param>
        public RhythmGuidePostEffectPresenter(
            PlayerAttackController playerAttackController,
            IRhythmGuideBeatViewModel beatViewModel,
            IRhythmGuidePostEffectViewModel viewModel)
        {
            _playerAttackController = playerAttackController
                ?? throw new ArgumentNullException(nameof(playerAttackController));
            _beatViewModel = beatViewModel ?? throw new ArgumentNullException(nameof(beatViewModel));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            _playerAttackController.OnAttackExecuted += AttackExecutedHandler;
        }

        /// <summary>
        ///     攻撃実行通知の購読を解除する。
        /// </summary>
        public void Dispose()
        {
            if (_playerAttackController == null)
            {
                return;
            }

            _playerAttackController.OnAttackExecuted -= AttackExecutedHandler;
            _playerAttackController = null;
        }

        private readonly IRhythmGuideBeatViewModel _beatViewModel;
        private readonly IRhythmGuidePostEffectViewModel _viewModel;
        private PlayerAttackController _playerAttackController;

        /// <summary>
        ///     攻撃入力時にジャスト成否とビート色をViewModelへ送る。
        /// </summary>
        /// <param name="attackName"> 実行された攻撃名。演出の出し分けには使用しない。 </param>
        /// <param name="hasHit"> 敵にヒットしたか。演出はヒット有無に依らず入力に対して返すため使用しない。 </param>
        private void AttackExecutedHandler(string attackName, bool hasHit)
        {
            // ビート色を取得できない場合はガイドが未構築のため、演出を出さない。
            if (!_beatViewModel.TryGetCurrentBeatColor(out Color color))
            {
                return;
            }

            // ガイド上のJustTimingMarkerにカーソルが乗っている入力のみをジャストとして扱う。
            _viewModel.Play(new RhythmGuidePostEffectDto(_beatViewModel.IsOnJustTiming, color));
        }
    }
}
