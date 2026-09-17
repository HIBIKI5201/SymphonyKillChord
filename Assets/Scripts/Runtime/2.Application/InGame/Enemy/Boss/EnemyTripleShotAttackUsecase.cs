using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の3方向攻撃のUsecaseクラス。
    /// </summary>
    public class EnemyTripleShotAttackUsecase
    {
        /// <summary>
        ///     コンストラクター。
        /// </summary>
        /// <param name="raycastDectector"></param>
        public EnemyTripleShotAttackUsecase(EnemyRaycastDetectService raycastDectector)
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
                Debug.LogError("[EnemyTripleShotAttackUsecase] attackDefinition is null");
                return;
            }

            Debug.Log($"[EnemyTripleShotAttackUsecase] ExecuteAttack 開始 Attack={attackDefinition?.AttackName}");

            if (_raycastDetector.CanRaycastHitTarget)
            {
                AttackResult result = AttackExecutor.Execute(attackDefinition, attacker, defender, false, _baseDamage);
                Debug.Log($"[EnemyTripleShotAttackUsecase] ExecuteAttack 完了 Damage={result.FinalDamage.Value}");
            }
        }
        private Damage _baseDamage = new Damage(10); // TODO敵の基礎攻撃力があるはずなので、それを使用するようにする。
        private readonly EnemyRaycastDetectService _raycastDetector;
    }
}
