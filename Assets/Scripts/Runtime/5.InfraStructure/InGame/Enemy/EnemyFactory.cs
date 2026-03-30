using KillChord.Runtime.Domain;

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

        public EnemyMusicSpec CreateEnemyMusicSpec(EnemyMusicData enemyMusicData)
        {
            return new EnemyMusicSpec(
                enemyMusicData.BarFlag,
                enemyMusicData.TimeSignature,
                enemyMusicData.TargetBeat);
        }

        public EnemyAttackMusicSpec CreateEnemyAttackMusicSpec(
            EnemyMusicData encounterData,
            EnemyMusicData battleMusicData)
        {
            return new EnemyAttackMusicSpec(
                CreateEnemyMusicSpec(encounterData),
                CreateEnemyMusicSpec(battleMusicData));
        }
    }
}
