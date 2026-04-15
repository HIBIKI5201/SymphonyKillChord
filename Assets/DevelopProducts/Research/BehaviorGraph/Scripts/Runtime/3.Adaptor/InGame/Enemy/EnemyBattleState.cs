using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle
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
        }

        public CharacterEntity Attacker { get; }
        public CharacterEntity Target { get; }
        public AttackDefinition CurrentAttack { get; }

        public bool IsInAttackRange { get; private set; }

        public void EnterRange() => IsInAttackRange = true;
        public void ExitRange() => IsInAttackRange = false;
    }
}
