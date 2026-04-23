using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
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
                new AttackRangeMin(enemyMoveData.AttackRangeMin),
                new AttackRangeMax(enemyMoveData.AttackRangeMax));
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
