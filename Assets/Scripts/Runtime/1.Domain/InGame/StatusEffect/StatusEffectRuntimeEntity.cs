using System;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     付与中の状態効果と有効期限を保持するエンティティです。
    /// </summary>
    public class StatusEffectRuntimeEntity
    {
        public StatusEffectRuntimeEntity(IStatusEffect effect, float currentTime)
        {
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));

            RefreshDuration(effect.Duration, currentTime);
        }

        /// <summary> 付与中の状態効果です。 </summary>
        public IStatusEffect Effect { get; private set; }

        /// <summary>
        ///     指定した時間で期限切れかどうかを判定します。
        /// </summary>
        /// <param name="currentTime"> 判定する現在時刻。 </param>
        /// <returns> 期限切れの場合はtrue、それ以外の場合はfalse。 </returns>
        public bool IsExpired(float currentTime)
        {
            return !_isUntilRemoved && currentTime >= _expireAt;
        }

        /// <summary>
        ///     効果時間を延長します。
        /// </summary>
        /// <param name="duration"> 延長する継続時間。 </param>
        public void ExtendDuration(StatusEffectDuration duration)
        {
            if (_isUntilRemoved)
            {
                return;
            }

            if (duration.IsUntilRemoved)
            {
                _isUntilRemoved = true;
                _expireAt = float.PositiveInfinity;
                return;
            }

            _expireAt += duration.Seconds;
        }

        /// <summary>
        ///     効果時間を更新します。
        /// </summary>
        /// <param name="duration"> 更新する継続時間。 </param>
        /// <param name="currentTime"> 現在時刻。 </param>
        public void RefreshDuration(StatusEffectDuration duration, float currentTime)
        {
            _isUntilRemoved = duration.IsUntilRemoved;

            _expireAt = duration.IsUntilRemoved
                ? float.PositiveInfinity
                : currentTime + duration.Seconds;
        }

        /// <summary>
        ///     状態効果を置き換えます。
        /// </summary>
        /// <param name="effect"> 置き換える状態効果。 </param>
        /// <param name="currentTime"> 現在時刻。 </param>
        public void Replace(IStatusEffect effect, float currentTime)
        {
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));

            RefreshDuration(effect.Duration, currentTime);
        }

        /// <summary>
        ///     残りの継続時間を取得します。
        /// </summary>
        /// <param name="currentTime"> 現在時刻。 </param>
        /// <returns> 残りの継続時間。 </returns>
        public float GetRemainingDuration(float currentTime)
        {
            return _isUntilRemoved ? float.PositiveInfinity : Math.Max(0f, _expireAt - currentTime);
        }

        private bool _isUntilRemoved;
        private float _expireAt;
    }
}
