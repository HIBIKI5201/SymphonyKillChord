using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃を実行するユースケースクラス。
    /// </summary>
    public class EnemyAttackUsecase
    {
        public EnemyAttackUsecase(IMusicSyncService musicSyncService)
        {
            _musicSyncService = musicSyncService;
        }

        public AttackResult ExecuteAttack(AttackDefinition attackDefinition,
            IAttacker attacker,
            IDefender defender)
        {
            Debug.Log($"[EnemyAttackUsecase] ExecuteAttack 開始 Attack={attackDefinition?.AttackName}");

            AttackResult result = AttackExecutor.Execute(attackDefinition, attacker, defender);

            Debug.Log($"[EnemyAttackUsecase] ExecuteAttack 完了 Damage={result.FinalDamage.Value}");
       
            _musicSyncService.RegisterBattleActionHistory(BattleActionType.Attack);
            return result;
        }

        private readonly IMusicSyncService _musicSyncService;
    }
}
