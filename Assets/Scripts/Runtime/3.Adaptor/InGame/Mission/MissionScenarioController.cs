using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     Missionモジュールによるのシナリオ再生を行うコントローラーです。
    /// </summary>
    public sealed class MissionScenarioController : IDisposable
    {
        public MissionScenarioController(
            MissionRuntimeService missionRuntimeService,
            ObjectiveSequenceClearCondition objectiveSequence,
            IScenarioPlaybackService scenarioPlaybackService,
            IScenarioBattlePauseController battlePauseController,
            IScenarioInputModeController inputModeController)
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(nameof(missionRuntimeService));
            _objectiveSequence = objectiveSequence
                ?? throw new ArgumentNullException(nameof(objectiveSequence));
            _scenarioPlaybackService = scenarioPlaybackService
                ?? throw new ArgumentNullException(nameof(scenarioPlaybackService));
            _battlePauseController = battlePauseController
                ?? throw new ArgumentNullException(nameof(battlePauseController));
            _inputModeController = inputModeController
                ?? throw new ArgumentNullException(nameof(inputModeController));
        }

        /// <summary> シナリオ再生を開始したときに発火するイベント </summary>
        public event Action OnScenarioPlaybackStarted;

        /// <summary> シナリオ再生を終了したときに発火するイベント </summary>
        public event Action OnScenarioPlaybackEnded;

        /// <summary>
        ///     Missionイベントの購読を開始し、現在のステップを評価します。
        /// </summary>
        public void Start()
        {
            if (_isStarted || _isDisposed)
            {
                return;
            }

            _isStarted = true;
            _missionRuntimeService.OnObjectiveStepChanged += HandleObjectiveStepChanged;
            HandleObjectiveStepChanged(_missionRuntimeService.MissionProgress.ObjectiveStepIndex);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _missionRuntimeService.OnObjectiveStepChanged -= HandleObjectiveStepChanged;

            if (_isPlaybackInProgress)
            {
                _scenarioPlaybackService.RequestSkip();
            }
        }

        /// <summary>
        ///     目標ステップがシナリオ再生条件なら、シナリオを一度だけ開始します。
        /// </summary>
        /// <param name="stepIndex">開始した目標ステップのIndexです。</param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            if (_isDisposed || _isPlaybackInProgress)
            {
                return;
            }

            ObjectiveSequenceStep step = _objectiveSequence.GetStep(stepIndex);
            ScenarioPlaybackClearCondition condition =
                step != null ? ClearConditionChain.Find<ScenarioPlaybackClearCondition>(step.Condition) : null;

            if (condition == null || condition.IsPlaybackCompleted)
            {
                return;
            }

            _isPlaybackInProgress = true;
            _ = PlayScenarioAsync(condition);
        }

        /// <summary>
        ///     戦闘を停止してシナリオを再生し、終了後に目標条件と戦闘状態を更新します。
        /// </summary>
        /// <param name="condition">再生対象の目標条件です。</param>
        /// <returns>シナリオ再生処理です。</returns>
        private async Task PlayScenarioAsync(ScenarioPlaybackClearCondition condition)
        {
            bool isScenarioPauseStarted = false;
            bool isScenarioPlaybackStarted = false;
            try
            {
                isScenarioPauseStarted = _battlePauseController.BeginScenarioPause();
                if (!isScenarioPauseStarted)
                {
                    Debug.LogError($"[MissionScenarioController] ポーズできなかったため、シナリオを開始しません。");
                    _isPlaybackInProgress = false;
                    return;
                }

                _inputModeController.EnterScenarioInputMode();
                isScenarioPlaybackStarted = true;
                OnScenarioPlaybackStarted?.Invoke();

                await _scenarioPlaybackService.PlayScenario(condition.ScenarioId);
                if (!_isDisposed)
                {
                    condition.CompletePlayback();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (isScenarioPlaybackStarted)
                {
                    _inputModeController.ExitScenarioInputMode();
                    OnScenarioPlaybackEnded?.Invoke();
                }

                if (isScenarioPauseStarted)
                {
                    _battlePauseController.EndScenarioPause();
                }

                _isPlaybackInProgress = false;
            }
        }
        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        private readonly IScenarioPlaybackService _scenarioPlaybackService;
        private readonly IScenarioBattlePauseController _battlePauseController;
        private readonly IScenarioInputModeController _inputModeController;
        private bool _isPlaybackInProgress;
        private bool _isStarted;
        private bool _isDisposed;
    }
}
