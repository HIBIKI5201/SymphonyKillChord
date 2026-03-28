using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     ScriptableObjectからDomainを生成する。
    /// </summary>
    public class EnemyFactory
    {
        public EnemyMoveSpec CreateEnemyMoveSpec(EnemyMoveData enemyMoveData)
        {
            return new EnemyMoveSpec(
                new MoveSpeed(enemyMoveData.MoveSpeed),
                new AttackRange(enemyMoveData.AttackRange));
        }
    }
}
