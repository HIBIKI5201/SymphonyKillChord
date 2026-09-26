using System;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     状態効果の継続時間を表す値オブジェクト。
    /// </summary>
    public readonly struct StatusEffectDuration
    {
        private StatusEffectDuration(float seconds, bool isPermanent)
        {
            Seconds = seconds;
            IsUntilRemoved = isPermanent;
        }

        /// <summary> 明示的に解除されるまで継続する状態効果。 </summary>
        public static StatusEffectDuration UntilRemoved { get; } = new StatusEffectDuration(0f, true);

        /// <summary> 継続秒数。永続時は0です。 </summary>
        public float Seconds { get; }

        /// <summary> 明示的に解除されるまで継続する状態効果かどうか。 </summary>
        public bool IsUntilRemoved { get; }

        /// <summary>
        ///     秒数から継続時間を作成します。
        /// </summary>
        /// <param name="seconds"> 継続秒数。 </param>
        /// <returns> 継続時間。 </returns>
        public static StatusEffectDuration FromSeconds(float seconds)
        {
            if (!float.IsFinite(seconds) || seconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds),
                    "状態効果の継続時間は0以上の有限の値である必要があります。");
            }

            return new StatusEffectDuration(seconds, false);
        }
    }
}
