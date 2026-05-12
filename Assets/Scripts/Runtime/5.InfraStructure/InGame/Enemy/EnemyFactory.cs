using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     ScriptableObjectからDomainを生成する。
    /// </summary>
    public static class EnemyFactory
    {
        /// <summary>
        ///     敵の移動情報を生成する。
        /// </summary>
        /// <param name="enemyMoveData"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static EnemyMoveSpec CreateEnemyMoveSpec(EnemyMoveData enemyMoveData)
        {
            if(enemyMoveData == null)
            {
                throw new System.ArgumentNullException(nameof(enemyMoveData), "敵移動データがNULLです。");
            }
            if(enemyMoveData.AttackRangeMin > enemyMoveData.AttackRangeMax)
            {
                throw new System.ArgumentException("攻撃範囲の最小値と最大値が整合していません。");
            }
            return new EnemyMoveSpec(
                new MoveSpeed(enemyMoveData.MoveSpeed),
                new AttackRangeMin(enemyMoveData.AttackRangeMin),
                new AttackRangeMax(enemyMoveData.AttackRangeMax));
        }

        /// <summary>
        ///     敵の音楽同期タイミング情報を生成する。
        /// </summary>
        /// <param name="enemyMusicData"></param>
        /// <returns></returns>
        public static EnemyMusicSpec CreateEnemyMusicSpec(EnemyMusicData enemyMusicData)
        {
            return new EnemyMusicSpec(
                enemyMusicData.BarFlag,
                enemyMusicData.TimeSignature,
                enemyMusicData.TargetBeat);
        }

        /// <summary>
        ///     敵の攻撃に関する音楽同期タイミング情報を生成する。
        /// </summary>
        /// <param name="encounterData"></param>
        /// <param name="battleMusicData"></param>
        /// <returns></returns>
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
