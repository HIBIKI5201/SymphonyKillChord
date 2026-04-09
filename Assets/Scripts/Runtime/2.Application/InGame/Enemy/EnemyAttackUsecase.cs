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
        /// 敵の攻撃を実行するユースケースクラスのインスタンスを生成する。
        /// </summary>
        /// <param name="musicSyncService"></param>
        public EnemyAttackUsecase(IMusicSyncService musicSyncService)
        {
            _musicSyncService = musicSyncService;
        }

        /// <summary>
        ///     攻撃を実行する。
        ///     履歴の登録も行う。
        /// </summary>
        /// <param name="attackDefinition"></param>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public AttackResult ExecuteAttack(
            AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender
            )
        {
            if (attackDefinition == null)
            {
                Debug.LogError("[EnemyAttackUsecase] attackDefinition is null");
                return default;
            }

            Debug.Log($"[EnemyAttackUsecase] ExecuteAttack 開始 Attack={attackDefinition?.AttackName}");

            AttackResult result = AttackExecutor.Execute(attackDefinition, attacker, defender);

            Debug.Log($"[EnemyAttackUsecase] ExecuteAttack 完了 Damage={result.FinalDamage.Value}");

            _musicSyncService.RegisterBattleActionHistory(BattleActionType.Attack);
            return result;
        }

        private readonly IMusicSyncService _musicSyncService;
    }
}
