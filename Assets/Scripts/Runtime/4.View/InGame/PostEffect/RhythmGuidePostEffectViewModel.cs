using KillChord.Runtime.Adaptor.InGame.PostEffect;
using KillChord.Runtime.View.InGame.Music;
using System;

namespace KillChord.Runtime.View.InGame.PostEffect
{
    /// <summary>
    ///     リズムガイドの全画面Vignetteについて、演出設定を参照して表示内容を決定しViewへ反映するViewModelです。
    /// </summary>
    public sealed class RhythmGuidePostEffectViewModel : IRhythmGuidePostEffectViewModel
    {
        /// <summary>
        ///     反映先のViewと演出設定を受け取る。
        /// </summary>
        /// <param name="postEffectView"> 全画面演出View。 </param>
        /// <param name="effectConfig"> リズムガイドの演出設定。 </param>
        public RhythmGuidePostEffectViewModel(
            RhythmGuidePostEffectView postEffectView,
            ACLikeRhythmGuideEffectConfig effectConfig)
        {
            if (postEffectView == null)
            {
                throw new ArgumentNullException(nameof(postEffectView));
            }

            if (effectConfig == null)
            {
                throw new ArgumentNullException(nameof(effectConfig));
            }

            _postEffectView = postEffectView;
            _effectConfig = effectConfig;
        }

        /// <summary>
        ///     ジャスト成否に応じた強さと時間で全画面Vignetteを再生する。
        /// </summary>
        /// <param name="dto"> 反映する表示データ。 </param>
        public void Play(in RhythmGuidePostEffectDto dto)
        {
            if (!_effectConfig.IsVignetteEnabled || _postEffectView == null)
            {
                return;
            }

            // ジャスト時は濃く長く、通常入力時は控えめに出して打鍵感の差を付ける。
            float intensity = dto.IsJustTiming
                ? _effectConfig.VignetteIntensity
                : _effectConfig.NormalVignetteIntensity;
            float duration = dto.IsJustTiming
                ? _effectConfig.VignetteDuration
                : _effectConfig.NormalVignetteDuration;

            _postEffectView.SetColor(dto.Color);
            _postEffectView.PlayOneShot(_effectConfig.VignetteEase, duration, intensity);
        }

        private readonly RhythmGuidePostEffectView _postEffectView;
        private readonly ACLikeRhythmGuideEffectConfig _effectConfig;
    }
}
