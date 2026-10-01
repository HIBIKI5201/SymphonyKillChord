using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Utility.Diagnostics;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の3方向攻撃のUsecaseクラス。
    /// </summary>
    public class EnemyTripleShotAttackUseCase
    {
        /// <summary>
        ///     コンストラクター。
        /// </summary>
        /// <param name="raycastDectector"></param>
        public EnemyTripleShotAttackUseCase(EnemyRaycastDetectService raycastDectector)
        {
            _raycastDetector = raycastDectector;
        }

        /// <summary>
        ///     攻撃を実行する。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public void ExecuteAttack(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender
            )
        {
            if (attackDefinition == null)
            {
                Debug.LogError("[EnemyTripleShotAttackUseCase] attackDefinition is null");
                return;
            }

            DevLog.Log($"[EnemyTripleShotAttackUseCase] ExecuteAttack 開始 Attack={attackDefinition?.AttackName}");

            if (_raycastDetector.CanRaycastHitTarget)
            {
                AttackResult result = AttackExecutor.Execute(
                    attackDefinition, attacker, defender, false, attacker.BaseDamage);
                DevLog.Log($"[EnemyTripleShotAttackUseCase] ExecuteAttack 完了 Damage={result.FinalDamage.Value}");
            }
        }
        private readonly EnemyRaycastDetectService _raycastDetector;
    }
}
