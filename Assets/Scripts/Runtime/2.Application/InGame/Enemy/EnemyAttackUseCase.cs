using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃を実行するユースケースクラス。
    /// </summary>
    public class EnemyAttackUseCase
    {
        /// <summary>
        ///     敵の攻撃を実行するユースケースクラスのインスタンスを生成する。
        /// </summary>
        /// <param name="raycastDectector"></param>
        public EnemyAttackUseCase(EnemyRaycastDetectService raycastDectector)
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
                Debug.LogError("[EnemyAttackUseCase] attackDefinition is null");
                return;
            }

            Debug.Log($"[EnemyAttackUseCase] ExecuteAttack 開始 Attack={attackDefinition?.AttackName}");

            if (_raycastDetector.CanRaycastHitTarget)
            {
                AttackResult result = AttackExecutor.Execute(
                    attackDefinition, attacker, defender, false, _baseDamage);
                Debug.Log($"[EnemyAttackUseCase] ExecuteAttack 完了 Damage={result.FinalDamage.Value}");
            }
        }
        private Damage _baseDamage = new Damage(10); // TODO敵の基礎攻撃力があるはずなので、それを使用するようにする。
        private readonly EnemyRaycastDetectService _raycastDetector;
    }
}
