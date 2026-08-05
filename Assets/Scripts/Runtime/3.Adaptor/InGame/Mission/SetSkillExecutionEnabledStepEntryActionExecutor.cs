using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using KillChord.Runtime.Domain.InGame.Player;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     スキル発動可否を切り替える目標ステップ進入時アクションを実行するクラスです。
    /// </summary>
    public sealed class SetSkillExecutionEnabledStepEntryActionExecutor : IMissionStepEntryActionExecutor
    {
        /// <summary>
        ///     SetSkillExecutionEnabledStepEntryActionExecutor クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="playerActionRestriction">プレイヤーの行動制限状態</param>
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
                    $"{nameof(entryAction)} must be {nameof(SetSkillExecutionEnabledStepEntryAction)}.",
                    nameof(entryAction));
            }
            if (skillExecutionAction.IsSkillExecutionEnabled)
            {
                Debug.Log("[SetSkillExecutionEnabledStepEntryActionExecutor] スキル発動可能");
                _playerActionRestriction.RemoveSkillRestriction(PlayerActionRestrictionReason.Tutorial);
            }
            else
            {
                Debug.Log("[SetSkillExecutionEnabledStepEntryActionExecutor] スキル発動不可");
                _playerActionRestriction.AddSkillRestriction(PlayerActionRestrictionReason.Tutorial);
            }
        }

        /// <summary> スキル発動可否を切り替えるControllerです。 </summary>
        private readonly PlayerActionRestrictionState _playerActionRestriction;
    }
}
