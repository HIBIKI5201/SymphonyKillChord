using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
using System.Collections.Generic;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵モジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class EnemyModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="enemyWaveSpawnerState"> 敵ウェーブ状態です。 </param>
        public EnemyModuleContainer(EnemyWaveSpawnerState enemyWaveSpawnerState)
        {
            EnemyWaveSpawnerState = enemyWaveSpawnerState;
        }

        /// <summary> 敵ウェーブ状態です。 </summary>
        public EnemyWaveSpawnerState EnemyWaveSpawnerState { get; }

        /// <summary> 敵ウェーブ制御です。 </summary>
        public EnemyWaveSpawnerController EnemyWaveSpawnerController { get; set; }

        /// <summary> 互換用のステージ演出定義カタログです。 </summary>
        public IReadOnlyDictionary<int, IStageEffectDefinition> StageEffectCatalog { get; set; }

        /// <summary> ボス初期化クラスです。 </summary>
        public BossInitializer BossInitializer { get; set; }
    }
}
