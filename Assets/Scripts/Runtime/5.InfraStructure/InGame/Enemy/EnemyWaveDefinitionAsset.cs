using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
using KillChord.Runtime.InfraStructure.InGame.Stage;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     1ステージ分の敵Wave定義。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyWavesDefinitionAsset", menuName = "KillChord/Enemy/" + nameof(EnemyWaveDefinitionAsset))]
    public class EnemyWaveDefinitionAsset : ScriptableObject
    {
        /// <summary> 敵Wave定義IDです。 </summary>
        public EnemyWaveDefinitionId Id => new EnemyWaveDefinitionId(_id.Id);

        /// <summary>
        ///     1ステージ分の敵Wave定義を生成する。
        /// </summary>
        /// <returns></returns>
        public EnemyWaves ToDefinition()
        {
            SingleWaveDefinition[] sourceWaves = _waves ?? Array.Empty<SingleWaveDefinition>();
            EnemyWaveDefinition[] waves = new EnemyWaveDefinition[sourceWaves.Length];
            for (int i = 0; i < sourceWaves.Length; i++)
            {
                WaveDetailDefinition[] sourceDetails = sourceWaves[i].Details ?? Array.Empty<WaveDetailDefinition>();
                EnemyWaveDetail[] details = new EnemyWaveDetail[sourceDetails.Length];
                for (int j = 0; j < sourceDetails.Length; j++)
                {
                    EnemyWaveDetail detail = new EnemyWaveDetail(sourceDetails[j].EnemyType, sourceDetails[j].EnemyAmount);
                    details[j] = detail;
                }

                List<int> stageEffectIds = new();
                if (sourceWaves[i].StageEffects != null)
                {
                    for (int j = 0; j < sourceWaves[i].StageEffects.Count; j++)
                    {
                        StageEffectAssetBase stageEffect = sourceWaves[i].StageEffects[j];
                        if (stageEffect != null)
                        {
                            stageEffectIds.Add(stageEffect.EffectId);
                        }
                    }
                }

                DataID[] sourceSpawnPointCandidates =
                    sourceWaves[i].SpawnPointCandidates ?? Array.Empty<DataID>();
                List<int> spawnPointCandidateHashes = new(sourceSpawnPointCandidates.Length);
                for (int j = 0; j < sourceSpawnPointCandidates.Length; j++)
                {
                    spawnPointCandidateHashes.Add(sourceSpawnPointCandidates[j].Id);
                }

                EnemyWaveDefinition wave = new EnemyWaveDefinition(
                    details,
                    sourceWaves[i].WaveDuration,
                    stageEffectIds,
                    spawnPointCandidateHashes);
                waves[i] = wave;
            }

            EnemyWaves ret = new EnemyWaves(waves, _loop, _loopStart);
            return ret;
        }

        /// <summary>
        ///     既存Wave定義からステージ演出カタログを生成します。
        /// </summary>
        /// <returns> 演出IDをキーにしたカタログです。 </returns>
        public IReadOnlyDictionary<int, IStageEffectDefinition> CreateStageEffectCatalog()
        {
            Dictionary<int, IStageEffectDefinition> catalog = new();
            SingleWaveDefinition[] sourceWaves = _waves ?? Array.Empty<SingleWaveDefinition>();
            for (int i = 0; i < sourceWaves.Length; i++)
            {
                List<StageEffectAssetBase> stageEffects = sourceWaves[i].StageEffects;
                if (stageEffects == null)
                {
                    continue;
                }

                for (int j = 0; j < stageEffects.Count; j++)
                {
                    StageEffectAssetBase stageEffect = stageEffects[j];
                    if (stageEffect == null || catalog.ContainsKey(stageEffect.EffectId))
                    {
                        continue;
                    }

                    catalog.Add(stageEffect.EffectId, stageEffect.Create());
                }
            }

            return catalog;
        }

        [SerializeField, SourceDataCollection("Wave"), Tooltip("敵Wave定義を一意に識別するIDです。")]
        private DataID _id;

        [SerializeField, Tooltip("1Wave分の定義")]
        private SingleWaveDefinition[] _waves;
        [SerializeField, Tooltip("全Wave終了後、繰り返して生成するか")]
        private bool _loop;
        [SerializeField, Tooltip("繰り返す場合の開始Index")]
        private int _loopStart;

        [Serializable]
        private class WaveDetailDefinition
        {
            /// <summary> 敵種類 </summary>
            [Tooltip("敵種類")] public EnemyType EnemyType;
            /// <summary> 敵の数 </summary>
            [Tooltip("敵の生成数"), Range(0, 20)] public int EnemyAmount;
        }

        [Serializable]
        private class SingleWaveDefinition
        {
            /// <summary> 敵種類ごとの詳細 </summary>
            [Tooltip("敵種類ごとの定義")] public WaveDetailDefinition[] Details;
            /// <summary> 次Waveまでの時間 </summary>
            [Tooltip("Waveの継続時間(秒)"), Range(0, 1800)] public float WaveDuration;
            /// <summary> Wave開始時に予約するステージ演出です。 </summary>
            [SerializeReference, SubclassSelector, Tooltip("Wave開始時に音楽同期で実行するステージ演出です。")]
            public List<StageEffectAssetBase> StageEffects = new();
            /// <summary> このWaveで敵を生成する際の候補スポーンポイントです。空の場合は全スポーンポイントが対象です。 </summary>
            [Tooltip("このWaveで敵を生成する際の候補スポーンポイントです。空の場合は全スポーンポイントが対象です。")]
            public DataID[] SpawnPointCandidates = Array.Empty<DataID>();
        }
    }
}
