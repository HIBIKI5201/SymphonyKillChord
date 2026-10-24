using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using UnityEngine;
namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     敵の無敵状態を切り替える目標ステップ進入時アクションのアセットです。
    /// </summary>
    [Serializable]
    public class SetEnemyInvincibleStepEntryActionAsset : MissionStepEntryActionAssetBase
    {
        /// <inheritdoc />
        public override IMissionStepEntryAction Create()
        {
            return new SetEnemyInvincibleStepEntryAction(_isInvincible);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return _isInvincible ? "敵を無敵状態にする" : "敵の無敵状態を解除する";
        }

        [SerializeField, Tooltip("敵を無敵状態にする場合はtrue")]
        private bool _isInvincible = true;
    }
}
