using System;
using UnityEngine;

namespace KillChord.Runtime.Utility
{
    /// <summary>
    ///     イベント定義：ダメージを受ける時
    /// </summary>
    public readonly struct EOnTakeDamage : IEvent
    {
        public readonly float Damage;
        public readonly bool Critical;
        public readonly EntityId DefenderId;

        public EOnTakeDamage(float damage, bool critical, EntityId defenderId)
        {
            DefenderId = defenderId;
            Damage = damage;
            Critical = critical;
        }
    }
}
