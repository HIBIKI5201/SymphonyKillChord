using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     プレイヤー死亡失敗条件のアセットクラス。
    /// </summary>
    [Serializable]
    public class PlayerDeadFailConditionAsset : MissionFailConditionAssetBase
    {
        /// <summary>
        ///     失敗条件を生成します。
        /// </summary>
        /// <returns>失敗条件。</returns>
        public override IMissionFailCondition Create()
        {
            return new PlayerDeadFailCondition();
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            return $"プレイヤーが死んだら失敗する条件";
        }
    }
}
