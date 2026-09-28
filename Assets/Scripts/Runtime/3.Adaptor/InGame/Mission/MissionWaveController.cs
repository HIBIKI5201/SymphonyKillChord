using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     Missionと敵Waveの開始・全滅通知を仲介するControllerです。
    /// </summary>
    public sealed class MissionWaveController : IDisposable
    {
        /// <summary>
        ///     Missionへ敵Waveの開始制御と全滅通知を結合します。
        /// </summary>
        /// <param name="missionRuntimeService"> Missionのランタイムサービスです。 </param>
        /// <param name="objectiveSequence"> Missionの目標シーケンスです。 </param>
        /// <param name="waveSpawnerController"> 敵Wave生成Controllerです。 </param>
        /// <param name="waveSpawnerState"> 敵Waveの進行状態です。 </param>
        /// <param name="waveTimerView"> 敵Waveタイマーの抽象です。 </param>
        public MissionWaveController(
            MissionRuntimeService missionRuntimeService,
            ObjectiveSequenceClearCondition objectiveSequence,
            EnemyWaveSpawnerController waveSpawnerController,
            EnemyWaveSpawnerState waveSpawnerState,
            IEnemyWaveTimerView waveTimerView)
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(nameof(missionRuntimeService));
            _objectiveSequence = objectiveSequence
                ?? throw new ArgumentNullException(nameof(objectiveSequence));
            _waveSpawnerController = waveSpawnerController
                ?? throw new ArgumentNullException(nameof(waveSpawnerController));
            _waveSpawnerState = waveSpawnerState
                ?? throw new ArgumentNullException(nameof(waveSpawnerState));
            _waveTimerView = waveTimerView
                ?? throw new ArgumentNullException(nameof(waveTimerView));

            _controlsWaveStart = _objectiveSequence.HasStepWithCondition<WaveStartClearCondition>();
            if (_controlsWaveStart)
            {
                _waveTimerView.SetAutoSpawnSuppressed(true);
                _missionRuntimeService.OnObjectiveStepChanged += HandleObjectiveStepChanged;
            }

            _waveSpawnerState.OnWaveAllCleared += HandleAllEnemyWavesCleared;
        }

        /// <summary>
        ///     Mission・Waveイベントの購読とWave自動生成の抑制を解除します。
        /// </summary>
        public void Dispose()
        {
            _waveSpawnerState.OnWaveAllCleared -= HandleAllEnemyWavesCleared;
            if (_controlsWaveStart)
            {
                _missionRuntimeService.OnObjectiveStepChanged -= HandleObjectiveStepChanged;
                _waveTimerView.SetAutoSpawnSuppressed(false);
            }
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        private readonly EnemyWaveSpawnerController _waveSpawnerController;
        private readonly EnemyWaveSpawnerState _waveSpawnerState;
        private readonly IEnemyWaveTimerView _waveTimerView;
        private readonly bool _controlsWaveStart;

        /// <summary>
        ///     すべての敵Waveの撃破完了をMissionへ伝えます。
        /// </summary>
        private void HandleAllEnemyWavesCleared()
        {
            _missionRuntimeService.OnEnemyWavesCleared();
        }

        /// <summary>
        ///     Waveステップの開始時に次の敵Waveを生成します。
        /// </summary>
        /// <param name="stepIndex"> 開始した目標ステップのIndexです。 </param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            ObjectiveSequenceStep step = _objectiveSequence.GetStep(stepIndex);
            if (step == null || ClearConditionChain.Find<WaveStartClearCondition>(step.Condition) == null)
            {
                return;
            }

            _waveSpawnerController.SpawnNextWave();
        }
    }
}
