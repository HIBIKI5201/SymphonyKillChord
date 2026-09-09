using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using UnityEngine;
namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     敵の戦闘AIの有効/無効を切り替える目標ステップ進入時アクションのアセットです。
    /// </summary>
    [Serializable]
    public class ToggleEnemyBattleAiStepEntryActionAsset : MissionStepEntryActionAssetBase
    {
        /// <inheritdoc />
        public override IMissionStepEntryAction Create()
        {
            return new ToggleEnemyBattleAIStepEntryAction(_isBattleAiActive);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return _isBattleAiActive ? "敵戦闘AIを有効にする" : "敵戦闘AIを無効にする";
        }

        [SerializeField, Tooltip("敵戦闘AIを有効にするの場合はtrue")]
        private bool _isBattleAiActive = true;
    }
}
