using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     内側の達成条件をラップし、このステップの間だけプレイヤーへバフ・デバフを付与する目印を兼ねるクリア条件です。
    ///     達成判定自体は内側の条件に委譲します。付与・解除のタイミング制御はMission側のControllerが担います。
    /// </summary>
    public sealed class PlayerBuffClearCondition : IMissionClearCondition, IObjectiveSequenceStepCondition, IObjectiveProgressReporter, IDecoratorClearCondition
    {
        /// <summary>
        ///     PlayerBuffClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="innerCondition"> 達成判定を委譲する内側の条件です。 </param>
        /// <param name="statusEffects"> このステップの間だけプレイヤーへ付与するバフ・デバフです。 </param>
        public PlayerBuffClearCondition(IMissionClearCondition innerCondition, IReadOnlyList<IStatusEffect> statusEffects)
        {
            _innerCondition = innerCondition ?? throw new ArgumentNullException(nameof(innerCondition));
            StatusEffects = statusEffects ?? Array.Empty<IStatusEffect>();
        }

        /// <summary> このステップの間だけプレイヤーへ付与する状態効果です。 </summary>
        public IReadOnlyList<IStatusEffect> StatusEffects { get; }

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
