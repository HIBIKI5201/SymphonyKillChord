using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     経過時間失敗条件（タイムリミット等）のアセットクラス。
    /// </summary>
    [Serializable]
    public class ElapsedTimeFailConditionAsset : MissionFailConditionAssetBase
    {
        /// <summary>
        ///     失敗条件を生成します。
        /// </summary>
        /// <returns>失敗条件。</returns>
        public override IMissionFailCondition Create()
        {
            return new ElapsedTimeFailCondition(_requiredElapsedTime);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            return $"{_requiredElapsedTime}秒を超えたら失敗する条件";
        }

        [SerializeField, Tooltip("この秒数を超えると失敗となります。")] private float _requiredElapsedTime;
    }
}
