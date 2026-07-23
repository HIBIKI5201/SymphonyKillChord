using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     IDに対応する敵Wave定義を提供するリポジトリです。
    /// </summary>
    public interface IEnemyWaveDefinitionRepository
    {
        /// <summary>
        ///     IDに対応する敵Wave進行データを生成します。
        /// </summary>
        /// <param name="id"> 取得する敵Wave定義IDです。 </param>
        /// <param name="enemyWaves"> 生成した敵Wave進行データです。 </param>
        /// <returns> IDに対応する定義が存在する場合はtrueです。 </returns>
        bool TryCreateEnemyWaves(
            EnemyWaveDefinitionId id,
            out EnemyWaves enemyWaves);
    }
}
