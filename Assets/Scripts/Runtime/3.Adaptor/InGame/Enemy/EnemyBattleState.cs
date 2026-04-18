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
            CharacterEntity target,
            AttackDefinition currentAttack)
        {
            Attacker = attacker;
            Target = target;
            CurrentAttack = currentAttack;
            FirstAttack = false;
        }

        public CharacterEntity Attacker { get; }
        public CharacterEntity Target { get; }
        public AttackDefinition CurrentAttack { get; }

        public bool IsInAttackRange { get; private set; }
        public bool FirstAttack { get; private set; }

        public void EnterRange() => IsInAttackRange = true;
        public void ExitRange() => IsInAttackRange = false;
        public void AttackExcuted() => FirstAttack = false;
    }
}
