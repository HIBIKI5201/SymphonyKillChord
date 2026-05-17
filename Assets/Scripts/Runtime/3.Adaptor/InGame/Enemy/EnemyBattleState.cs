using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵Aiの攻撃に関する戦闘状態をまとめたクラス。
    /// </summary>
    public class EnemyBattleState
    {
        public EnemyBattleState(CharacterEntity attacker,
            CharacterEntity target,
            AttackDefinition currentAttack)
        {
            Attacker = attacker;
            Target = target;
            CurrentAttack = currentAttack;
            FirstAttack = true;
            IsStunned = false;
        }

        /// <summary> 攻撃者（自身）のエンティティ </summary>
        public CharacterEntity Attacker { get; }
        /// <summary> 攻撃目標のエンティティ </summary>
        public CharacterEntity Target { get; }
        /// <summary> 攻撃情報 </summary>
        public AttackDefinition CurrentAttack { get; }

        /// <summary> 攻撃目標が攻撃可能な範囲に居るか </summary>
        public bool IsInAttackRange { get; private set; }
        /// <summary> 初回攻撃か </summary>
        public bool FirstAttack { get; private set; }
        /// <summary> 硬直中か </summary>
        public bool IsStunned { get; private set; }

        /// <summary> 攻撃目標が攻撃範囲に入った </summary>
        public void EnterRange() => IsInAttackRange = true;
        /// <summary> 攻撃目標が攻撃範囲から出た </summary>
        public void ExitRange() => IsInAttackRange = false;
        /// <summary> 攻撃を実行した </summary>
        public void AttackExcuted() => FirstAttack = false;
        /// <summary> 硬直発生 </summary>
        public void Stunned() => IsStunned = true;
        /// <summary> 硬直から回復した </summary>
        public void StunRecover() => IsStunned = false;
    }
}
