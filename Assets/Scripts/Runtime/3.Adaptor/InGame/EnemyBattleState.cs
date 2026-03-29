using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     敵Aiの攻撃に関する戦闘状態をまとめたクラス。
    /// </summary>
    public class EnemyBattleState
    {
        public EnemyBattleState(CharacterEntity attacker, 
            IHitTarget target,
            AttackId attackId)
        {
            Attacker = attacker;
            Target = target;
            AttackId = attackId;
        }

        public CharacterEntity Attacker { get; }
        public IHitTarget Target { get; }
        public AttackId AttackId { get; }

        public bool IsInAttackRange { get; private set; }

        public void EnterRange() => IsInAttackRange = true;
        public void ExitRange() => IsInAttackRange = false;
    }
}
