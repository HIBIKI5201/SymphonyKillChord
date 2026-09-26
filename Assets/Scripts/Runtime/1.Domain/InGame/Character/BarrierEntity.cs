using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクターが保持するバリアを表すエンティティ。
    /// </summary>
    public class BarrierEntity
    {
        /// <summary> 現在のバリア値を取得する。 </summary>
        public float CurrentValue { get; private set; }

        /// <summary>
        ///     バリアを追加する。
        /// </summary>
        /// <param name="amount"> 追加するバリアの量。 </param>
        public void Add(float amount)
        {
            if (!float.IsFinite(amount) || amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "バリアの追加量は有限の非負の数でなければなりません。");
            }

            CurrentValue += amount;
        }

        /// <summary>
        ///     ダメージを吸収する。
        /// </summary>
        /// <param name="damage"> 吸収するダメージ。 </param>
        /// <param name="absorbedDamage"> 吸収されたダメージ。 </param>
        /// <returns> 吸収後の残りのダメージ。 </returns>
        public Damage Absorb(
            Damage damage,
            out Damage absorbedDamage)
        {
            float absorbedValue = Math.Min(CurrentValue, damage.Value);
            CurrentValue -= absorbedValue;

            absorbedDamage = new Damage(absorbedValue);
            return new Damage(damage.Value - absorbedValue);
        }

        /// <summary>
        ///     バリアをクリアする。
        /// </summary>
        public void Clear()
        {
            CurrentValue = 0f;
        }
    }
}
