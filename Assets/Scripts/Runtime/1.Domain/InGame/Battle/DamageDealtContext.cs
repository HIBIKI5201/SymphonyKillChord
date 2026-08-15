using KillChord.Runtime.Utility.Persistent;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     ダメージが与えられた際の情報を保持する構造体。
    /// </summary>
    public readonly struct DamageDealtContext
    {
        public DamageDealtContext(
            IAttacker attacker, IDefender defender,
            AttackResult attackResult, DamageAttackType attackType)
        {
            Attacker = attacker;
            Defender = defender;
            AttackResult = attackResult;
            AttackType = attackType;
        }

        /// <summary> 攻撃者。 </summary>
        public IAttacker Attacker { get; }

        /// <summary> 防御者。 </summary>
        public IDefender Defender { get; }

        /// <summary> 攻撃結果。 </summary>
        public AttackResult AttackResult { get; }

        /// <summary> 攻撃の種類。 </summary>
        public DamageAttackType AttackType { get; }
    }
}
