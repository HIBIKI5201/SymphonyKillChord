using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     防御者を表すインターフェース。
    /// </summary>
    public interface IDefender
    {
        /// <summary>
        ///     HPに変化があった時に発火するイベント。<br/>
        ///     引数は、現在HP、最大HP、変化量（ダメージは負、回復は正）
        /// </summary>
        event Action<float, float, float> OnHealthChanged;

        /// <summary>
        ///     現在の体力を取得する。
        /// </summary>
        Health CurrentHealth { get; }

        /// <summary>
        ///     体力の最大値を取得する。
        /// </summary>
        Health MaxHealth { get; }

        /// <summary> 防御者がダメージを受けられるかどうかを取得する。 </summary>
        bool CanTakeDamage { get; }

        /// <summary>
        ///     防御者側のバフシステムを取得する。
        /// </summary>
        IBuffSystem BuffSystem { get; }

        /// <summary> 状態効果システムを取得する。 </summary>
        IStatusEffectSystem StatusEffectSystem { get; }

        /// <summary>
        ///     ダメージを受ける。
        /// </summary>
        /// <param name="damage"> 受けるダメージ </param>
        /// <returns> 実際に受けたダメージ </returns>
        Damage TakeDamage(Damage damage);
    }
}
