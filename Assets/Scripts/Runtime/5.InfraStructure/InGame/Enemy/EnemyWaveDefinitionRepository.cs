using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
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
    public sealed class EnemyWaveDefinitionRepository : ScriptableObject, IEnemyWaveDefinitionRepository
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
            if (TryGetDefinitionAsset(id, out EnemyWaveDefinitionAsset definitionAsset))
            {
                enemyWaves = definitionAsset.ToDefinition();
                return true;
            }

            enemyWaves = null;
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
            if (TryGetDefinitionAsset(id, out EnemyWaveDefinitionAsset definitionAsset))
            {
                stageEffectCatalog = definitionAsset.CreateStageEffectCatalog();
                return true;
            }

            stageEffectCatalog = null;
            return false;
        }

        [SerializeField, Tooltip("IDで取得可能にする敵Wave定義アセットの一覧です。")]
        private EnemyWaveDefinitionAsset[] _waveDefinitionAssets;

        private Dictionary<EnemyWaveDefinitionId, EnemyWaveDefinitionAsset> _definitionAssetMap;

        /// <summary>
        ///     Inspector上の設定が変わった際に検索用辞書を破棄します。
        /// </summary>
        private void OnValidate()
        {
            _definitionAssetMap = null;
        }

        /// <summary>
        ///     IDに対応する敵Wave定義アセットを取得します。
        /// </summary>
        /// <param name="id"> 取得する敵Wave定義IDです。 </param>
        /// <param name="definitionAsset"> 取得した敵Wave定義アセットです。 </param>
        /// <returns> IDに対応するアセットが存在する場合はtrueです。 </returns>
        private bool TryGetDefinitionAsset(
            EnemyWaveDefinitionId id,
            out EnemyWaveDefinitionAsset definitionAsset)
        {
            EnsureDefinitionAssetMap();
            return _definitionAssetMap.TryGetValue(id, out definitionAsset);
        }

        /// <summary>
        ///     敵Wave定義IDをキーとする検索用辞書を構築します。
        /// </summary>
        private void EnsureDefinitionAssetMap()
        {
            if (_definitionAssetMap != null)
            {
                return;
            }

            _definitionAssetMap = new Dictionary<EnemyWaveDefinitionId, EnemyWaveDefinitionAsset>();
            if (_waveDefinitionAssets == null)
            {
                return;
            }

            for (int i = 0; i < _waveDefinitionAssets.Length; i++)
            {
                EnemyWaveDefinitionAsset definitionAsset = _waveDefinitionAssets[i];
                if (definitionAsset == null)
                {
                    continue;
                }

                EnemyWaveDefinitionId id = definitionAsset.Id;
                if (id.Value == 0)
                {
                    Debug.LogWarning(
                        $"[{nameof(EnemyWaveDefinitionRepository)}] 敵Wave定義IDが未設定です。"
                        + $" Asset: {definitionAsset.name}",
                        this);
                    continue;
                }

                if (_definitionAssetMap.ContainsKey(id))
                {
                    Debug.LogWarning(
                        $"[{nameof(EnemyWaveDefinitionRepository)}] 敵Wave定義IDが重複しています。"
                        + $" Id: {id.Value}, Asset: {definitionAsset.name}",
                        this);
                    continue;
                }

                _definitionAssetMap.Add(id, definitionAsset);
            }
        }
    }
}
