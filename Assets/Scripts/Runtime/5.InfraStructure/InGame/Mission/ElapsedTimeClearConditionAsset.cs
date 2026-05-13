using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     経過時間クリア条件（生存ミッション等）のアセットクラス。
    /// </summary>
    [Serializable]
    public class ElapsedTimeClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <summary>
        ///     クリア条件を生成します。
        /// </summary>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create()
        {
            return new ElapsedTimeClearCondition(_requiredElapsedTime);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            return $"{_requiredElapsedTime}秒生存する条件";
        }

        [SerializeField, Tooltip("この秒数間生存するとクリアとなります。")] private float _requiredElapsedTime;
    }
}
