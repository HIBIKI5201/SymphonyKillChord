using KillChord.Runtime.Utility.Persistent;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     ダメージを受けた際の文脈情報を表す構造体。
    /// </summary>
    public readonly struct DamageTakenContext
    {
        public DamageTakenContext(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult,
            DamageAttackType attackType)
        {
            Attacker = attacker;
            Defender = defender;
            AttackResult = attackResult;
            AttackType = attackType;
        }

        /// <summary> 攻撃者を取得する。 </summary>
        public IAttacker Attacker { get; }

        /// <summary> 防御者を取得する。 </summary>
        public IDefender Defender { get; }

        /// <summary> 攻撃の結果を取得する。 </summary>
        public AttackResult AttackResult { get; }

        /// <summary> 攻撃の種類を取得する。 </summary>
        public DamageAttackType AttackType { get; }
    }
}
