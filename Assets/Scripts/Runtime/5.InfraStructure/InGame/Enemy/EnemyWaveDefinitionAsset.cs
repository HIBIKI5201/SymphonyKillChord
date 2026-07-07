using KillChord.Runtime.Domain.InGame.Enemy;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     1ステージ分の敵Wave定義。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyWavesDefinitionAsset", menuName = "KillChord/Enemy/" + nameof(EnemyWaveDefinitionAsset))]
    public class EnemyWaveDefinitionAsset : ScriptableObject
    {
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
                EnemyWaveDefinition wave = new EnemyWaveDefinition(details, sourceWaves[i].WaveDuration);
                waves[i] = wave;
            }
            EnemyWaves ret = new EnemyWaves(waves, _loop, _loopStart);
            return ret;
        }

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
            [Tooltip("敵種類")]public EnemyType EnemyType;
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
        }
    }
}
