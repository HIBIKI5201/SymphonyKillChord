using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     敵Wave定義アセットをIDで検索するリポジトリです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(EnemyWaveDefinitionRepository),
        menuName = "KillChord/Enemy/" + nameof(EnemyWaveDefinitionRepository))]
    public sealed class EnemyWaveDefinitionRepository
        : ScriptableObjectRepositoryBase<EnemyWaveDefinitionId, EnemyWaveDefinitionAsset, EnemyWaveDefinitionAsset>,
            IEnemyWaveDefinitionRepository
    {
        /// <summary>
        ///     IDに対応する敵Wave進行データを生成します。
        /// </summary>
        /// <param name="id"> 取得する敵Wave定義IDです。 </param>
        /// <param name="enemyWaves"> 生成した敵Wave進行データです。 </param>
        /// <returns> IDに対応する定義が存在する場合はtrueです。 </returns>
        public bool TryCreateEnemyWaves(
            EnemyWaveDefinitionId id,
            out EnemyWaves enemyWaves)
        {
            if (TryFind(id, out EnemyWaveDefinitionAsset definitionAsset))
            {
                enemyWaves = definitionAsset.ToDefinition();
                return true;
            }

            enemyWaves = null;
            return false;
        }

        /// <summary>
        ///     IDに対応する敵Wave定義が使用するバトルシーン名を取得します。
        /// </summary>
        /// <param name="id"> 取得する敵Wave定義IDです。 </param>
        /// <param name="battleSceneName"> 取得したバトルシーン名です。 </param>
        /// <returns> IDに対応する定義が存在し、シーン名が設定されている場合はtrueです。 </returns>
        public bool TryGetBattleSceneName(
            EnemyWaveDefinitionId id,
            out string battleSceneName)
        {
            if (TryFind(id, out EnemyWaveDefinitionAsset definitionAsset)
                && !string.IsNullOrWhiteSpace(definitionAsset.BattleSceneName))
            {
                battleSceneName = definitionAsset.BattleSceneName;
                return true;
            }

            battleSceneName = null;
            return false;
        }

        /// <summary>
        ///     IDに対応するステージ演出カタログを生成します。
        /// </summary>
        /// <param name="id"> 取得する敵Wave定義IDです。 </param>
        /// <param name="stageEffectCatalog"> 生成したステージ演出カタログです。 </param>
        /// <returns> IDに対応する定義が存在する場合はtrueです。 </returns>
        public bool TryCreateStageEffectCatalog(
            EnemyWaveDefinitionId id,
            out IReadOnlyDictionary<int, IStageEffectDefinition> stageEffectCatalog)
        {
            if (TryFind(id, out EnemyWaveDefinitionAsset definitionAsset))
            {
                stageEffectCatalog = definitionAsset.CreateStageEffectCatalog();
                return true;
            }

            stageEffectCatalog = null;
            return false;
        }

        [SerializeField, Tooltip("IDで取得可能にする敵Wave定義アセットの一覧です。")]
        private EnemyWaveDefinitionAsset[] _waveDefinitionAssets;

        protected override IReadOnlyList<EnemyWaveDefinitionAsset> GetEntries() => _waveDefinitionAssets;

        protected override bool TryBuild(
            EnemyWaveDefinitionAsset entry,
            out EnemyWaveDefinitionId id,
            out EnemyWaveDefinitionAsset value)
        {
            id = entry.Id;
            value = entry;

            if (id.Value == 0)
            {
                Debug.LogWarning(
                    $"[{nameof(EnemyWaveDefinitionRepository)}] 敵Wave定義IDが未設定です。"
                    + $" Asset: {entry.name}",
                    this);
                return false;
            }

            return true;
        }
    }
}
