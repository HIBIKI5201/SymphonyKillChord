using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     防御者を表すインターフェース。
    /// </summary>
    public interface IDefender
    {
        /// <summary> ダメージを受ける時に発火するイベント。 </summary>
        public event Action<HealthEntity, float> OnDamageTaken;
        /// <summary> 回復した時に発火するイベント。 </summary>
        public event Action<HealthEntity, float> OnHealed;
        /// <summary>
        ///     現在の体力を取得する。
        /// </summary>
        public Health CurrentHealth { get; }

        /// <summary>
        ///     体力の最大値を取得する。
        /// </summary>
        public Health MaxHealth { get; }

        /// <summary>
        ///     ダメージを受ける。
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(Damage damage);
    }
}
