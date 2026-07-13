using System;
using KillChord.Runtime.Adaptor.InGame.Animation;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションの瞬間イベントを伝達するSignal。
    /// </summary>
    public sealed class CharacterAnimationSignal : ICharacterAnimationSignal
    {
        /// <summary>
        ///     Signalを初期化する。
        /// </summary>
        /// <param name="playbackMap"> 再生定義です。 </param>
        /// <param name="animationSpeedProvider"> 現在の再生速度取得処理です。 </param>
        /// <param name="timingCalculator"> 時間計算処理です。 </param>
        public CharacterAnimationSignal(
            CharacterAnimationPlaybackMap playbackMap,
            Func<float> animationSpeedProvider,
            CharacterAnimationOneShotTimingCalculator timingCalculator)
        {
            _playbackMap = playbackMap;
            _animationSpeedProvider = animationSpeedProvider;
            _timingCalculator = timingCalculator ?? new CharacterAnimationOneShotTimingCalculator();
        }

        /// <summary> 回避アニメーションの再生終了イベントです。 </summary>
        public event Action OnDodgeEnded;

        /// <summary>
        ///     回避アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public float RequestDodge()
        {
            return RequestOneShot(_playbackMap.Dodge, true);
        }

        /// <summary>
        ///     攻撃アニメーションの再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 置き換えたいアニメーションキー。未指定時は既定の攻撃アニメーション。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(string animationKey = null)
        {
            if (!string.IsNullOrWhiteSpace(animationKey)
                && _playbackMap.TryGetOneShotIndex(animationKey, out int oneShotIndex))
            {
                return RequestOneShot(oneShotIndex, false);
            }

            return RequestOneShot(_playbackMap.Attack, false);
        }

        /// <summary>
        ///     攻撃BeatTypeに対応するアニメーションの再生を要求します。
        /// </summary>
        /// <param name="attackType"> 攻撃結果のBeatTypeです。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(int attackType)
        {
            if (_playbackMap.TryGetAttackIndex(attackType, out int attackIndex))
            {
                return RequestOneShot(attackIndex, false);
            }

            return RequestOneShot(_playbackMap.Attack, false);
        }

        /// <summary>
        ///     任意キーのワンショットアニメーション再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 再生したいアニメーションキー。 </param>
        /// <param name="duration"> 再生時間です。 </param>
        /// <returns> 要求できた場合はtrue。 </returns>
        public bool TryRequestOneShot(string animationKey, out float duration)
        {
            duration = 0f;
            if (string.IsNullOrWhiteSpace(animationKey))
            {
                return false;
            }

            if (!_playbackMap.TryGetOneShotIndex(animationKey, out int oneShotIndex))
            {
                return false;
            }

            duration = RequestOneShot(oneShotIndex, false);
            return true;
        }

        /// <summary>
        ///     内部再生要求イベントです。
        /// </summary>
        internal event Action<CharacterAnimationRequest> OnRequested;

        /// <summary>
        ///     回避アニメーション再生終了を通知する。
        /// </summary>
        internal void NotifyDodgeEnded()
        {
            OnDodgeEnded?.Invoke();
        }

        /// <summary>
        ///     指定インデックスのワンショット再生を要求する。
        /// </summary>
        /// <param name="index"> 再生インデックスです。 </param>
        /// <param name="shouldNotifyDodgeEnded"> 回避終了通知が必要ならtrue。 </param>
        /// <returns> 現在速度を加味した再生時間です。 </returns>
        private float RequestOneShot(int index, bool shouldNotifyDodgeEnded)
        {
            float clipLength = _playbackMap.GetClipLength(index);
            float enterBlendDuration = _timingCalculator.GetEnterBlendDurationSeconds(
                clipLength,
                _playbackMap.GetEnterBlendFrameCount(index));
            float exitBlendDuration = _timingCalculator.GetExitBlendDurationSeconds(
                clipLength,
                _playbackMap.GetExitBlendFrameCount(index));

            OnRequested?.Invoke(new CharacterAnimationRequest(
                index,
                clipLength,
                enterBlendDuration,
                exitBlendDuration,
                shouldNotifyDodgeEnded));

            return _timingCalculator.GetPlaybackDurationSeconds(clipLength, GetAnimationSpeed());
        }

        /// <summary>
        ///     現在のアニメーション再生速度を取得する。
        /// </summary>
        /// <returns> 再生速度です。 </returns>
        private float GetAnimationSpeed()
        {
            return _animationSpeedProvider != null
                ? _animationSpeedProvider.Invoke()
                : 1f;
        }

        private readonly CharacterAnimationPlaybackMap _playbackMap;
        private readonly Func<float> _animationSpeedProvider;
        private readonly CharacterAnimationOneShotTimingCalculator _timingCalculator;
    }
}
