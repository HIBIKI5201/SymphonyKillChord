using UnityEngine;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     攻撃の基本情報を保持するクラス。
    ///     攻撃の種類や基本ダメージなど、攻撃に関する定数的な情報を管理するためのクラス。
    /// </summary>
    public class AttackDefinition
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public AttackDefinition(AttackId id, Damage baseDamage)
        {
            Id = id;
            BaseDamage = baseDamage;
        }

        public AttackId Id { get; }
        public Damage BaseDamage { get; }
    }
}
