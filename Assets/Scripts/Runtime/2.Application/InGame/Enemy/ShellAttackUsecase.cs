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
            AttackResult attackResult = AttackExecutor.Execute(
                attackDefinition, attacker, defender, false, _baseDamage,
                damageUnitMultiplier: PLAYER_HEALTH_UNIT_MULTIPLIER);
            Debug.Log($"[ShellAttackUsecase] ExecuteAttack 完了 Damage={attackResult.FinalDamage.Value}");
        }

        // プレイヤーHPの桁変更に合わせ、既存の攻撃計算後に適用する。
        private const float PLAYER_HEALTH_UNIT_MULTIPLIER = 10f;

        private Damage _baseDamage = new Damage(10);// TODO敵の基礎攻撃力があるはずなので、それを使用するようにする。
    }
}
