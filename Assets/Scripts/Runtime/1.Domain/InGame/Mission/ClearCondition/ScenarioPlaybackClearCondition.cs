using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     指定したシナリオの再生完了を待つ目標ステップ条件です。
    ///     再生の実行はMission側のControllerが担い、完了時に<see cref="CompletePlayback"/>を呼び出します。
    /// </summary>
    public sealed class ScenarioPlaybackClearCondition : IMissionClearCondition, IObjectiveSequenceStepCondition, IObjectiveProgressReporter, IDecoratorClearCondition
    {
        /// <summary>
        ///     ScenarioPlaybackClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="scenarioId">再生するシナリオIDです。</param>
        public ScenarioPlaybackClearCondition(IMissionClearCondition innerCondition, string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                throw new ArgumentException("シナリオIDが設定されていません。", nameof(scenarioId));
            }
            _innerCondition = innerCondition ?? throw new ArgumentNullException(nameof(innerCondition));
            _scenarioId = scenarioId;
        }

        /// <summary> 再生するシナリオIDです。 </summary>
        public string ScenarioId => _scenarioId;

        /// <inheritdoc />
        public IMissionClearCondition InnerCondition => _innerCondition;
        
        /// <summary> シナリオ再生が完了しているか。 </summary>
        public bool IsPlaybackCompleted { get; private set; }

        /// <inheritdoc />
        public bool IsSatisfied(MissionProgress progress)
        {
            return IsPlaybackCompleted && _innerCondition.IsSatisfied(progress);
        }

        /// <inheritdoc />
        public string GetDescription()
        {
            return _innerCondition.GetDescription();
        }

        /// <inheritdoc />
        public void BeginStep(MissionProgress progress)
        {
            IsPlaybackCompleted = false;
            if (_innerCondition is IObjectiveSequenceStepCondition stepCondition)
            {
                stepCondition.BeginStep(progress);
            }
        }

        /// <summary>
        ///     シナリオ再生の完了を記録します。
        /// </summary>
        public void CompletePlayback()
        {
            IsPlaybackCompleted = true;
        }
        public int CurrentCount(MissionProgress progress)
        {
            return _innerCondition is IObjectiveProgressReporter reporter ? reporter.CurrentCount(progress) : 0;
        }

        /// <inheritdoc />
        public int RequiredCount => _innerCondition is IObjectiveProgressReporter reporter ? reporter.RequiredCount : 0;

        private readonly IMissionClearCondition _innerCondition;
        private readonly string _scenarioId;
    }
}
