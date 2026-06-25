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
            EnemyWaveDefinition[] waves = new EnemyWaveDefinition[_waves.Length];
            for(int i = 0; i < _waves.Length; i++)
            {
                EnemyWaveDetail[] details = new EnemyWaveDetail[_waves[i].Details.Length];
                for (int j = 0; j < _waves[i].Details.Length; j++)
                {
                    EnemyWaveDetail detail = new EnemyWaveDetail(_waves[i].Details[j].EnemyType, _waves[i].Details[j].EnemyAmount);
                    details[j] = detail;
                }
                EnemyWaveDefinition wave = new EnemyWaveDefinition(details, _waves[i].WaveDuration);
                waves[i] = wave;
            }
            EnemyWaves ret = new EnemyWaves(waves, _loop, _loopStart);
            return ret;
        }

        [SerializeField]
        private SingleWaveDefinition[] _waves;
        [SerializeField]
        private bool _loop;
        [SerializeField]
        private int _loopStart;

        [Serializable]
        private class WaveDetailDefinition
        {
            /// <summary> 敵種類 </summary>
            public EnemyType EnemyType;
            /// <summary> 敵の数 </summary>
            [Range(0, 20)] public int EnemyAmount;
        }

        [Serializable]
        private class SingleWaveDefinition
        {
            /// <summary> 敵種類ごとの詳細 </summary>
            public WaveDetailDefinition[] Details;
            /// <summary> 次Waveまでの時間 </summary>
            [Range(0, 1800)] public float WaveDuration;
        }
    }
}
