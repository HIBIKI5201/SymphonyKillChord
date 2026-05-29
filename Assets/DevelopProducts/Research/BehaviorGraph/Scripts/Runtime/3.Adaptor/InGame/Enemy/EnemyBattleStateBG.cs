using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     敵Aiの攻撃に関する戦闘状態をまとめたクラス。
    /// </summary>
    public class EnemyBattleStateBG
    {
        public EnemyBattleStateBG(CharacterEntity attacker,
            CharacterEntity target,
            AttackDefinition currentAttack)
        {
            EnemyStatus = EnumEnemyStatus.Idle;
            Attacker = attacker;
            Target = target;
            CurrentAttack = currentAttack;
        }

        public CharacterEntity Attacker { get; }
        public CharacterEntity Target { get; }
        public AttackDefinition CurrentAttack { get; }
        public EnumEnemyStatus EnemyStatus;

        public bool IsInAttackRange { get; private set; }

        public void EnterRange() => IsInAttackRange = true;
        public void ExitRange() => IsInAttackRange = false;

        public void SetEnemyStatus(EnumEnemyStatus status)
        {
            EnemyStatus = status;
        }

        public void AddEnemyStatus(EnumEnemyStatus status)
        {
            EnemyStatus |= status;
        }

        public void RemoveEnemyStatus(EnumEnemyStatus status)
        {
            EnemyStatus &= ~status;
        }

        public bool HasEnemyStatus(EnumEnemyStatus status)
        {
            return (EnemyStatus & status) == status;
        }
    }
}
