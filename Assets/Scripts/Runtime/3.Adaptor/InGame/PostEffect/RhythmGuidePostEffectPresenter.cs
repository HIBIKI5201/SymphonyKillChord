using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
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
        /// <param name="beatViewModel"> 拍種ごとのビート色の取得元。 </param>
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

            _playerAttackController.OnAttackBeatExecuted += AttackExecutedHandler;
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

            _playerAttackController.OnAttackBeatExecuted -= AttackExecutedHandler;
            _playerAttackController = null;
        }

        private readonly IRhythmGuideBeatViewModel _beatViewModel;
        private readonly IRhythmGuidePostEffectViewModel _viewModel;
        private PlayerAttackController _playerAttackController;

        /// <summary>
        ///     攻撃入力時にジャスト成否とビート色をViewModelへ送る。
        /// </summary>
        /// <param name="beatType"> 入力時に確定した攻撃の拍種。 </param>
        /// <param name="isJustHit"> 攻撃・スキルに適用したジャスト成否。 </param>
        private void AttackExecutedHandler(BeatType beatType, bool isJustHit)
        {
            // ビート色を取得できない場合はガイドが未構築のため、演出を出さない。
            if (!_beatViewModel.TryGetBeatColor((int)beatType, out Color color))
            {
                return;
            }

            // 履歴更新後の時刻や描画済みカーソルから再判定せず、入力結果をそのまま表示する。
            _viewModel.Play(new RhythmGuidePostEffectDto(isJustHit, color));
        }
    }
}
