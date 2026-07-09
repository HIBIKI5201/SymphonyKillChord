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
        public CharacterAnimationSignal(CharacterAnimationPlaybackMap playbackMap)
        {
            _playbackMap = playbackMap;
        }

        /// <summary> 回避アニメーションの再生終了イベントです。 </summary>
        public event Action OnDodgeEnded;

        /// <summary>
        ///     回避アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public float RequestDodge()
        {
            OnRequested?.Invoke(new CharacterAnimationRequest(_playbackMap.Dodge, true));
            return _playbackMap.DodgeDuration;
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
                OnRequested?.Invoke(new CharacterAnimationRequest(oneShotIndex, false));
                return _playbackMap.GetClipLength(oneShotIndex);
            }

            OnRequested?.Invoke(new CharacterAnimationRequest(_playbackMap.Attack, false));
            return _playbackMap.AttackDuration;
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

            duration = _playbackMap.GetClipLength(oneShotIndex);
            OnRequested?.Invoke(new CharacterAnimationRequest(oneShotIndex, false));
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

        private readonly CharacterAnimationPlaybackMap _playbackMap;
    }
}
