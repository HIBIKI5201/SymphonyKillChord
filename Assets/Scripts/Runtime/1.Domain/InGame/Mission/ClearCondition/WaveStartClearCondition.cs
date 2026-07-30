using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     内側の達成条件をラップし、このステップの開始時に次の敵Waveを生成する目印を兼ねるクリア条件です。
    ///     達成判定(＝Waveの完了)自体は内側の条件に委譲します。
    /// </summary>
    public sealed class WaveStartClearCondition : IMissionClearCondition, IObjectiveSequenceStepCondition, IObjectiveProgressReporter, IDecoratorClearCondition
    {
        /// <summary>
        ///     WaveStartClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="innerCondition"> Wave完了の判定を委譲する内側の条件です。 </param>
        public WaveStartClearCondition(IMissionClearCondition innerCondition)
        {
            _innerCondition = innerCondition ?? throw new ArgumentNullException(nameof(innerCondition));
        }

        /// <inheritdoc />
        public IMissionClearCondition InnerCondition => _innerCondition;

        /// <inheritdoc />
        public bool IsSatisfied(MissionProgress progress)
        {
            return _innerCondition.IsSatisfied(progress);
        }

        /// <inheritdoc />
        public string GetDescription()
        {
            return _innerCondition.GetDescription();
        }

        /// <inheritdoc />
        public void BeginStep(MissionProgress progress)
        {
            if (_innerCondition is IObjectiveSequenceStepCondition stepCondition)
            {
                stepCondition.BeginStep(progress);
            }
        }

        /// <inheritdoc />
        public int CurrentCount(MissionProgress progress)
        {
            return _innerCondition is IObjectiveProgressReporter reporter ? reporter.CurrentCount(progress) : 0;
        }

        /// <inheritdoc />
        public int RequiredCount => _innerCondition is IObjectiveProgressReporter reporter ? reporter.RequiredCount : 0;

        private readonly IMissionClearCondition _innerCondition;
    }
}
