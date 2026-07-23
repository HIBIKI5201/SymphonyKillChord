using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     MissionのWaveステップと敵Wave生成を仲介するControllerです。
    /// </summary>
    public sealed class MissionWaveController : IDisposable
    {
        /// <summary>
        ///     MissionのWaveステップと敵Wave生成を結合します。
        /// </summary>
        /// <param name="missionRuntimeService"> Missionのランタイムサービスです。 </param>
        /// <param name="objectiveSequence"> Waveステップを含む目標シーケンスです。 </param>
        /// <param name="waveSpawnerController"> 敵Wave生成Controllerです。 </param>
        /// <param name="waveTimerView"> 敵Waveタイマーの抽象です。 </param>
        public MissionWaveController(
            MissionRuntimeService missionRuntimeService,
            ObjectiveSequenceClearCondition objectiveSequence,
            EnemyWaveSpawnerController waveSpawnerController,
            IEnemyWaveTimerView waveTimerView)
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(nameof(missionRuntimeService));
            _objectiveSequence = objectiveSequence
                ?? throw new ArgumentNullException(nameof(objectiveSequence));
            _waveSpawnerController = waveSpawnerController
                ?? throw new ArgumentNullException(nameof(waveSpawnerController));
            _waveTimerView = waveTimerView
                ?? throw new ArgumentNullException(nameof(waveTimerView));

            _waveTimerView.SetAutoSpawnSuppressed(true);
            _missionRuntimeService.OnObjectiveStepChanged += HandleObjectiveStepChanged;
        }

        /// <summary>
        ///     Missionイベントの購読とWave自動生成の抑制を解除します。
        /// </summary>
        public void Dispose()
        {
            _missionRuntimeService.OnObjectiveStepChanged -= HandleObjectiveStepChanged;
            _waveTimerView.SetAutoSpawnSuppressed(false);
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        private readonly EnemyWaveSpawnerController _waveSpawnerController;
        private readonly IEnemyWaveTimerView _waveTimerView;

        /// <summary>
        ///     Waveステップの開始時に次の敵Waveを生成します。
        /// </summary>
        /// <param name="stepIndex"> 開始した目標ステップのIndexです。 </param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            if (_objectiveSequence.GetStep(stepIndex)
                    is not WaveObjectiveSequenceStep)
            {
                return;
            }

            _waveSpawnerController.SpawnNextWave();
        }
    }
}
