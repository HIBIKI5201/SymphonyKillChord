using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Domain.InGame.Enemy;
using System;

namespace KillChord.DebugWorld.Battle
{
    /// <summary>
    ///     裏世界から出撃した戦闘で、敵Waveを差し替えてセーブを止めるクラスです。
    /// </summary>
    public sealed class DebugBattleOverrides : IEnemyWavesOverride, ISaveSuppression
    {
        /// <summary>
        ///     Waveの並びと繰り返し開始位置で初期化します。
        /// </summary>
        /// <param name="waves"> Waveの並びです。 </param>
        /// <param name="loopStart"> 繰り返しを始めるWaveの位置です。 </param>
        private DebugBattleOverrides(EnemyWaveDefinition[] waves, int loopStart)
        {
            _waves = waves;
            _loopStart = loopStart;
        }

        /// <summary> 裏世界の戦闘は常にセーブを止めます。 </summary>
        public bool IsSaveSuppressed => true;

        /// <summary>
        ///     敵を出さない戦闘の設定を作ります。
        /// </summary>
        /// <returns> 作った設定です。 </returns>
        public static DebugBattleOverrides CreatePlayerControlTest()
        {
            return new DebugBattleOverrides(new[] { CreateEmptyWave() }, 0);
        }

        /// <summary>
        ///     指定した敵を、決まった間隔で指定した数だけ出し続ける戦闘の設定を作ります。
        /// </summary>
        /// <param name="enemyDefinitionId"> 出す敵の定義IDです。 </param>
        /// <param name="enemyAmount"> 1回に出す数です。 </param>
        /// <returns> 作った設定です。 </returns>
        public static DebugBattleOverrides CreateEnemyTest(EnemyDefinitionId enemyDefinitionId, int enemyAmount)
        {
            if (enemyAmount <= 0) { throw new ArgumentOutOfRangeException(nameof(enemyAmount)); }

            EnemyWaveDefinition enemyWave = new(
                new[] { new EnemyWaveDetail(enemyDefinitionId, enemyAmount) },
                WAVE_DURATION);
            return new DebugBattleOverrides(new[] { enemyWave }, 0);
        }

        /// <summary>
        ///     最初にボスを1体だけ出し、その後は敵を出さない戦闘の設定を作ります。
        ///     <para> ボスはシーンに1体だけ置かれているため、繰り返し出しません。 </para>
        /// </summary>
        /// <param name="bossDefinitionId"> 出すボスの定義IDです。 </param>
        /// <returns> 作った設定です。 </returns>
        public static DebugBattleOverrides CreateBossTest(EnemyDefinitionId bossDefinitionId)
        {
            EnemyWaveDefinition bossWave = new(
                new[] { new EnemyWaveDetail(bossDefinitionId, BOSS_AMOUNT) },
                WAVE_DURATION);
            return new DebugBattleOverrides(new[] { bossWave, CreateEmptyWave() }, 1);
        }

        /// <summary>
        ///     設定したWaveを、繰り返し開始位置から繰り返す敵Waveを生成します。
        /// </summary>
        /// <returns> 差し替える敵Waveです。 </returns>
        public EnemyWaves CreateEnemyWaves()
        {
            // 敵Waveは進行状態を持つため、出撃のたびに新しく作る。
            return new EnemyWaves(_waves, true, _loopStart);
        }

        private const float WAVE_DURATION = 30f;
        private const int BOSS_AMOUNT = 1;

        private readonly EnemyWaveDefinition[] _waves;
        private readonly int _loopStart;

        /// <summary>
        ///     敵を出さないWaveを作ります。
        /// </summary>
        /// <returns> 作ったWaveです。 </returns>
        private static EnemyWaveDefinition CreateEmptyWave()
        {
            return new EnemyWaveDefinition(Array.Empty<EnemyWaveDetail>(), WAVE_DURATION);
        }
    }
}
