using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using System;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃定義と攻撃処理のパイプラインを組み合わせて、攻撃処理全体を実行するクラス。
    /// </summary>
    public static class AttackExecutor
    {
        public static AttackResult Execute(AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender)
        {
            AttackResult result = AttackCalculator.Calculate(attackDefinition, attacker, defender);

            defender.TakeDamage(result.FinalDamage);

            Debug.Log(
                 $"[Attack] " +
                 $"Attacker:{attackDefinition.AttackName} " +
                 $"Damage:{result.FinalDamage.Value} " +
                 $"Critical:{result.IsCritical}");

            return result;
        }
    }
}
