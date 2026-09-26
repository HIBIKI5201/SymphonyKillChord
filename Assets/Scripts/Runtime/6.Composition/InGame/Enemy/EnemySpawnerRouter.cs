using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     個別の敵定義から処理種別を解決し、対応するスポナーへ生成を委譲します。
    /// </summary>
    public sealed class EnemySpawnerRouter : IEnemySpawner
    {
        /// <summary>
        ///     敵定義リポジトリと処理種別ごとのスポナーで初期化します。
        /// </summary>
        /// <param name="repository"> 個別の敵定義リポジトリです。 </param>
        /// <param name="infantrySpawner"> 歩兵スポナーです。 </param>
        /// <param name="artillerySpawner"> 砲兵スポナーです。 </param>
        public EnemySpawnerRouter(
            EnemyDefinitionRepository repository,
            IEnemySpawner infantrySpawner,
            IEnemySpawner artillerySpawner)
        {
            _repository = repository;
            _infantrySpawner = infantrySpawner;
            _artillerySpawner = artillerySpawner;
        }

        /// <summary>
        ///     個別の敵定義に対応するスポナーで敵を生成します。
        /// </summary>
        /// <param name="enemyDefinitionId"> 生成する個別の敵定義IDです。 </param>
        /// <param name="amount"> 生成数です。 </param>
        /// <param name="candidateSpawnPointHashes"> 候補とするスポーンポイントIDです。 </param>
        /// <param name="callback"> 生成完了時に呼ばれます。 </param>
        public void SpawnEnemy(
            EnemyDefinitionId enemyDefinitionId,
            int amount,
            IReadOnlyList<int> candidateSpawnPointHashes,
            Action callback)
        {
            if (!_repository.TryGetDefinition(enemyDefinitionId, out EnemyDefinitionAsset definition))
            {
                Debug.LogError(
                    $"[{nameof(EnemySpawnerRouter)}] 敵定義IDに対応するデータが見つかりません。"
                    + $" Id: {enemyDefinitionId.Value}");
                return;
            }

            IEnemySpawner spawner = definition.EnemyType switch
            {
                EnemyType.Infantry => _infantrySpawner,
                EnemyType.Artillery => _artillerySpawner,
                _ => null,
            };
            if (spawner == null)
            {
                Debug.LogError(
                    $"[{nameof(EnemySpawnerRouter)}] 未対応の敵処理種別です。 Type: {definition.EnemyType}");
                return;
            }

            spawner.SpawnEnemy(enemyDefinitionId, amount, candidateSpawnPointHashes, callback);
        }

        private readonly EnemyDefinitionRepository _repository;
        private readonly IEnemySpawner _infantrySpawner;
        private readonly IEnemySpawner _artillerySpawner;
    }
}
