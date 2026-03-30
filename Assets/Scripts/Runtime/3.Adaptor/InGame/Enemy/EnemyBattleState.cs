using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Battle
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
