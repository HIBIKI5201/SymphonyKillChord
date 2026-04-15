using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Enemy;

namespace DevelopProducts.BehaviorGraph.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     ScriptableObjectからDomainを生成する。
    /// </summary>
    public static class EnemyFactory
    {
        public static EnemyMoveSpec CreateEnemyMoveSpec(EnemyMoveData enemyMoveData)
        {
            return new EnemyMoveSpec(
                new MoveSpeed(enemyMoveData.MoveSpeed),
                new AttackRange(enemyMoveData.AttackRange));
        }

        public static EnemyMusicSpec CreateEnemyMusicSpec(EnemyMusicData enemyMusicData)
        {
            return new EnemyMusicSpec(
                enemyMusicData.BarFlag,
                enemyMusicData.TimeSignature,
                enemyMusicData.TargetBeat);
        }

        public static EnemyAttackMusicSpec CreateEnemyAttackMusicSpec(
            EnemyMusicData encounterData,
            EnemyMusicData battleMusicData)
        {
            return new EnemyAttackMusicSpec(
                CreateEnemyMusicSpec(encounterData),
                CreateEnemyMusicSpec(battleMusicData));
        }
    }
}
