using KillChord.Runtime.Domain.InGame.StatusEffect;

namespace KillChord.Runtime.Application.InGame.StatusEffect
{
    /// <summary>
    ///     状態効果の共通データを保持する基底クラス。
    /// </summary>
    public abstract class StatusEffectBase : IStatusEffect
    {
        /// <summary>
        ///     ID・分類・持続時間・再付与時の扱いを指定して生成する。
        /// </summary>
        protected StatusEffectBase(
            StatusEffectId id,
            StatusEffectCategory category,
            StatusEffectDuration duration,
            StatusEffectReapplyPolicy reapplyPolicy)
        {
            Id = id;
            Category = category;
            Duration = duration;
            ReapplyPolicy = reapplyPolicy;
        }

        /// <inheritdoc />
        public StatusEffectId Id { get; }

        /// <inheritdoc />
        public StatusEffectCategory Category { get; }

        /// <inheritdoc />
        public StatusEffectDuration Duration { get; }

        /// <inheritdoc />
        public StatusEffectReapplyPolicy ReapplyPolicy { get; }
    }
}
