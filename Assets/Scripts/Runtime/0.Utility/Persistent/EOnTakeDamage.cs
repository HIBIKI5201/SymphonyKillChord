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
        // TODO EntityのHashCodeは一時的な手段。将来的に、別途でIDを割り当てるか、Colliderの参照を持つようにするなどを検討すべき
        public readonly int DefenderHashCode;
        public EOnTakeDamage(float damage, bool critical, int defenderHashCode)
        {
            DefenderHashCode = defenderHashCode;
            Damage = damage;
            Critical = critical;
        }
    }
}
