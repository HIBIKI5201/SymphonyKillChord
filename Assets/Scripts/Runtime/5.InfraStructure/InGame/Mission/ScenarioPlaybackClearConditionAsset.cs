using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     シナリオ再生の完了を待つクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class ScenarioPlaybackClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <inheritdoc />
        public override IMissionClearCondition Create(EnemyMissionKeyRepository missionKeyRepository)
        {
            if (_innerCondition == null)
            {
                throw new InvalidOperationException($"{nameof(_innerCondition)} is required.");
            }
            if (string.IsNullOrEmpty(_scenarioId))
            {
                throw new InvalidOperationException($"{nameof(_scenarioId)} is required.");
            }
            return new ScenarioPlaybackClearCondition(_innerCondition.Create(missionKeyRepository), _scenarioId);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return string.IsNullOrWhiteSpace(_scenarioId) ? "シナリオ未設定" : $"シナリオ「{_scenarioId}」を再生する";
        }

        [SerializeReference, SubclassSelector, Tooltip("ポップアップが閉じる判定を委譲する内側のクリア条件。")]
        private MissionClearConditionAssetBase _innerCondition;
        
        [SerializeField, Tooltip("再生するシナリオのID。")]
        private string _scenarioId;
    }
}
