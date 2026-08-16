using System;
using UnityEngine;

namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：ダメージを受ける時。
    ///     発火元はプレイヤー側の攻撃処理のみのため、実質的に敵が被弾した時に発火する。
    ///     プレイヤーの被弾は <see cref="EOnPlayerTakeDamage"/> を使用する。
    /// </summary>
    public readonly struct EOnTakeDamage : IEvent
    {
        public readonly float Damage;
        public readonly bool Critical;
        public readonly Guid DefenderId;
        public readonly DamageAttackType AttackType;

        public EOnTakeDamage(float damage, bool critical, Guid defenderId, DamageAttackType attackType)
        {
            DefenderId = defenderId;
            Damage = damage;
            Critical = critical;
            AttackType = attackType;
        }
    }
}
