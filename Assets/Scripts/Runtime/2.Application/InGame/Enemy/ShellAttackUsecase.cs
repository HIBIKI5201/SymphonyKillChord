using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     砲弾の攻撃処理を行う。
    /// </summary>
    public class ShellAttackUsecase
    {
        /// <summary>
        ///     砲弾の攻撃処理を行う。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        public void ExecuteAttack(AttackDefinition attackDefinition, IAttacker attacker, IDefender defender)
        {
            AttackResult attackResult = AttackExecutor.Execute(attackDefinition, attacker, defender);
            Debug.Log($"[ShellAttackUsecase] ExecuteAttack 完了 Damage={attackResult.FinalDamage.Value}");
        }
    }
}
