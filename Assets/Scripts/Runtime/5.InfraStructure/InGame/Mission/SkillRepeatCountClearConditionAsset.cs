using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     キルコードを必要回数発動するクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class SkillRepeatCountClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <inheritdoc />
        public override IMissionClearCondition Create()
        {
            if (_requiredCount <= 0)
            {
                throw new InvalidOperationException($"{nameof(_requiredCount)} must be greater than 0.");
            }

            return new ActionRepeatCountClearCondition(MissionActionKind.Skill, _requiredCount);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return $"キルコードを{_requiredCount}回以上発動する条件";
        }

        [SerializeField, Min(1), Tooltip("クリアに必要なキルコード発動回数です。")]
        private int _requiredCount = 1;
    }
}
