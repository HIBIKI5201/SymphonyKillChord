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
        ///     敵を出さない戦闘の設定を作ります。
        /// </summary>
        public DebugBattleOverrides()
        {
            _hasEnemy = false;
        }

        /// <summary>
        ///     指定した敵を、決まった間隔で指定した数だけ出し続ける戦闘の設定を作ります。
        /// </summary>
        /// <param name="enemyDefinitionId"> 出す敵の定義IDです。 </param>
        /// <param name="enemyAmount"> 1回に出す数です。 </param>
        public DebugBattleOverrides(EnemyDefinitionId enemyDefinitionId, int enemyAmount)
        {
            if (enemyAmount <= 0) { throw new ArgumentOutOfRangeException(nameof(enemyAmount)); }

            _hasEnemy = true;
            _enemyDefinitionId = enemyDefinitionId;
            _enemyAmount = enemyAmount;
        }

        /// <summary> 裏世界の戦闘は常にセーブを止めます。 </summary>
        public bool IsSaveSuppressed => true;

        /// <summary>
        ///     1つのWaveを繰り返す敵Waveを生成します。敵なしの場合は空のWaveを繰り返します。
        /// </summary>
        /// <returns> 差し替える敵Waveです。 </returns>
        public EnemyWaves CreateEnemyWaves()
        {
            EnemyWaveDetail[] details = _hasEnemy
                ? new[] { new EnemyWaveDetail(_enemyDefinitionId, _enemyAmount) }
                : Array.Empty<EnemyWaveDetail>();
            EnemyWaveDefinition[] waves = { new EnemyWaveDefinition(details, WAVE_DURATION) };
            return new EnemyWaves(waves, true, 0);
        }

        private const float WAVE_DURATION = 30f;

        private readonly bool _hasEnemy;
        private readonly EnemyDefinitionId _enemyDefinitionId;
        private readonly int _enemyAmount;
    }
}
