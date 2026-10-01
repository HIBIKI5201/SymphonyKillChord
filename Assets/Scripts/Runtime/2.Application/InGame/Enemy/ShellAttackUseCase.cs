using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Utility.Diagnostics;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     砲弾の攻撃処理を行う。
    /// </summary>
    public class ShellAttackUseCase
    {
        /// <summary>
        ///     砲弾の攻撃処理を行う。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        public void ExecuteAttack(AttackDefinition attackDefinition, IAttacker attacker, IDefender defender)
        {
            AttackResult attackResult = AttackExecutor.Execute(
                attackDefinition, attacker, defender, false, attacker.BaseDamage);
            DevLog.Log($"[ShellAttackUseCase] ExecuteAttack 完了 Damage={attackResult.FinalDamage.Value}");
        }

    }
}
