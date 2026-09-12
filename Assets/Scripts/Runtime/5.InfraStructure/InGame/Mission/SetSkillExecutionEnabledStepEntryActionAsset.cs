using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     スキル発動の可否を切り替える目標ステップ進入時アクションのアセットです。
    /// </summary>
    [Serializable]
    public sealed class SetSkillExecutionEnabledStepEntryActionAsset : MissionStepEntryActionAssetBase
    {
        /// <inheritdoc />
        public override IMissionStepEntryAction Create()
        {
            return new SetSkillExecutionEnabledStepEntryAction(_isSkillExecutionEnabled);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return _isSkillExecutionEnabled ? "スキル発動可能にする" : "スキル発動不可にする";
        }

        [SerializeField, Tooltip("スキル発動可能の場合はtrue")]
        private bool _isSkillExecutionEnabled = true;
    }
}
