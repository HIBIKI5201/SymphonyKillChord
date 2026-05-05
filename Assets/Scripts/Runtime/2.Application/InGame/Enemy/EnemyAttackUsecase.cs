using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃を実行するユースケースクラス。
    /// </summary>
    public class EnemyAttackUsecase
    {
        /// <summary>
        ///     敵の攻撃を実行するユースケースクラスのインスタンスを生成する。
        /// </summary>
        /// <param name="raycastDectector"></param>
        public EnemyAttackUsecase(EnemyRaycastDetectService raycastDectector)
        {
            _raycastDetector = raycastDectector;
        }

        /// <summary>
        ///     攻撃を実行する。
        ///     履歴の登録も行う。
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
                Debug.LogError("[EnemyAttackUsecase] attackDefinition is null");
                return;
            }

            Debug.Log($"[EnemyAttackUsecase] ExecuteAttack 開始 Attack={attackDefinition?.AttackName}");

            if (_raycastDetector.CanRaycastHitTarget)
            {
                AttackResult result = AttackExecutor.Execute(attackDefinition, attacker, defender);
                Debug.Log($"[EnemyAttackUsecase] ExecuteAttack 完了 Damage={result.FinalDamage.Value}");
            }
        }

        private readonly EnemyRaycastDetectService _raycastDetector;
    }
}
