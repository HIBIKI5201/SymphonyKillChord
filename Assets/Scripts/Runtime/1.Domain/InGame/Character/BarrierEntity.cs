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
        ///     上限を超えない範囲でバリアを追加する。
        /// </summary>
        /// <param name="amount"> 追加するバリアの量。 </param>
        /// <param name="maxValue"> バリアの上限。 </param>
        public void Add(float amount, float maxValue)
        {
            if (!float.IsFinite(amount) || amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "バリアの追加量は有限の非負の数でなければなりません。");
            }

            if (!float.IsFinite(maxValue) || maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxValue),
                    "バリアの上限は有限の非負の数でなければなりません。");
            }

            // 上限を超えた分は捨てる。すでに上限を超えている場合は減らさない。
            CurrentValue = Math.Max(CurrentValue, Math.Min(CurrentValue + amount, maxValue));
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
