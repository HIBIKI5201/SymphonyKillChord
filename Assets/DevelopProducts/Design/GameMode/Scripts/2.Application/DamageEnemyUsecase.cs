using DevelopProducts.Design.GameMode.Domain;
using DevelopProducts.Design.GameMode.InfraStructure;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Application
{
    /// <summary>
    ///     敵にダメージを与えるユースケース。
    /// </summary>
    public class DamageEnemyUsecase
    {
        public DamageEnemyUsecase(StageRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public void Execute(EnemyRuntimeState enemy,EnemyDefinition definition, int damage)
        {
            if(enemy == null ||  definition == null)
            {
                Debug.LogError("EnemyRuntimeState or EnemyDefinition is null.");
                return;
            }

            bool wasAlive = !enemy.IsDead;
            enemy.TakeDamage(damage);

            if (wasAlive && enemy.IsDead)
            {
                _runtimeContext.EnemyKillStatics.RecordKill(definition);
            }
        }

        private readonly StageRuntimeContext _runtimeContext;
    }
}
