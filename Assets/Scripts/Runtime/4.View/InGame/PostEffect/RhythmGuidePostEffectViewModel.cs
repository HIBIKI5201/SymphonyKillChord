using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.View.InGame.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.PostEffect
{
    /// <summary>
    ///     攻撃入力に応じてリズムガイドの全画面Vignetteを再生するViewModelです。
    /// </summary>
    public sealed class RhythmGuidePostEffectViewModel : IDisposable
    {
        /// <summary>
        ///     ViewModelを生成し、攻撃実行の通知へ購読する。
        /// </summary>
        /// <param name="postEffectView"> 全画面演出View。 </param>
        /// <param name="rhythmGuideView"> リズムガイドView。ジャスト成否とビート色の取得元。 </param>
        /// <param name="effectConfig"> リズムガイドの演出設定。 </param>
        /// <param name="playerAttackController"> 攻撃実行の通知元。 </param>
        public RhythmGuidePostEffectViewModel(
            RhythmGuidePostEffectView postEffectView,
            ACLikeRhythmGuideView rhythmGuideView,
            ACLikeRhythmGuideEffectConfig effectConfig,
            PlayerAttackController playerAttackController)
        {
            _postEffectView = postEffectView;
            _rhythmGuideView = rhythmGuideView;
            _effectConfig = effectConfig;
            _playerAttackController = playerAttackController;

            if (_playerAttackController != null)
            {
                _playerAttackController.OnAttackExecuted += HandleAttackExecuted;
            }
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

            _playerAttackController.OnAttackExecuted -= HandleAttackExecuted;
            _playerAttackController = null;
        }

        private readonly RhythmGuidePostEffectView _postEffectView;
        private readonly ACLikeRhythmGuideView _rhythmGuideView;
        private readonly ACLikeRhythmGuideEffectConfig _effectConfig;
        private PlayerAttackController _playerAttackController;

        /// <summary>
        ///     攻撃入力時にジャスト成否へ応じた強さでVignetteを再生する。
        /// </summary>
        /// <param name="attackName"> 実行された攻撃名。演出には使用しない。 </param>
        /// <param name="hasHit"> 敵にヒットしたか。演出には使用しない。 </param>
        private void HandleAttackExecuted(string attackName, bool hasHit)
        {
            if (_postEffectView == null || _rhythmGuideView == null || _effectConfig == null)
            {
                return;
            }

            if (!_effectConfig.IsVignetteEnabled)
            {
                return;
            }

            if (!_rhythmGuideView.TryGetCurrentBeatColor(out Color color))
            {
                return;
            }

            // ガイド上のJustTimingMarkerにカーソルが乗っている入力のみをジャストとして扱う。
            bool isJustTiming = _rhythmGuideView.IsOnJustTiming;
            float intensity = isJustTiming
                ? _effectConfig.VignetteIntensity
                : _effectConfig.NormalVignetteIntensity;
            float duration = isJustTiming
                ? _effectConfig.VignetteDuration
                : _effectConfig.NormalVignetteDuration;

            _postEffectView.SetColor(color);
            _postEffectView.OneShotRatio(_effectConfig.VignetteEase, duration, intensity);
        }
    }
}
