using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using KillChord.Runtime.Domain.InGame.Player;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     スキル発動可否を切り替える目標ステップ進入時アクションを実行するクラスです。
    /// </summary>
    public sealed class SetSkillExecutionEnabledStepEntryActionExecutor : IMissionStepEntryActionExecutor
    {
        public SetSkillExecutionEnabledStepEntryActionExecutor(PlayerActionRestrictionState playerActionRestriction)
        {
            _playerActionRestriction = playerActionRestriction ?? throw new ArgumentNullException(nameof(playerActionRestriction));
        }

        /// <inheritdoc />
        public Type EntryActionType => typeof(SetSkillExecutionEnabledStepEntryAction);

        /// <inheritdoc />
        public void Execute(IMissionStepEntryAction entryAction)
        {
            if (entryAction is not SetSkillExecutionEnabledStepEntryAction skillExecutionAction)
            {
                throw new ArgumentException(
                    $"{nameof(entryAction)}の型が{nameof(SetSkillExecutionEnabledStepEntryAction)}ではない。",
                    nameof(entryAction));
            }
            if (skillExecutionAction.IsSkillExecutionEnabled)
            {
                // スキル発動可能
                _playerActionRestriction.RemoveSkillRestriction(PlayerActionRestrictionReason.Tutorial);
            }
            else
            {
                // スキル発動不可
                _playerActionRestriction.AddSkillRestriction(PlayerActionRestrictionReason.Tutorial);
            }
        }

        private readonly PlayerActionRestrictionState _playerActionRestriction;
    }
}
