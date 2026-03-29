using KillChord.Runtime.Domain;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     敵の攻撃を実行するユースケースクラス。
    /// </summary>
    public class EnemyAttackUsecase
    {
        public EnemyAttackUsecase(AttackExecutor attackExecutor,IMusicSyncService musicSyncService)
        {
            _attackExecutor = attackExecutor;
            _musicSyncService = musicSyncService;
        }

        public AttackResult ExecuteAttack(CharacterEntity attacker, IHitTarget target, AttackId attackId)
        {
            if (attacker == null || target == null)
            {
                Debug.LogError("Attacker or Target is null.");
                return default;
            }

            _musicSyncService.RegisterBattleActionHistory(ActionType.Attack);
            return _attackExecutor.Execute(attacker, target, attackId);
        }

        private readonly AttackExecutor _attackExecutor;
        private readonly IMusicSyncService _musicSyncService;
    }
}
