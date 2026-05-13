using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     敵撃破数クリア条件のアセットクラス。
    /// </summary>
    [Serializable]
    public class EnemyKillCountClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <summary>
        ///     クリア条件を生成します。
        /// </summary>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create()
        {
            if (_enemyMissionKeyAsset == null)
            {
                throw new InvalidOperationException($"{nameof(_enemyMissionKeyAsset)} is required.");
            }

            if (_requiredKillCount <= 0)
            {
                throw new InvalidOperationException($"{nameof(_requiredKillCount)} must be greater than 0.");
            }

            return new EnemyKillCountClearCondition(
                _enemyMissionKeyAsset.Id,
                _requiredKillCount,
                _enemyMissionKeyAsset.DisplayName);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            string enemyName = _enemyMissionKeyAsset != null ? _enemyMissionKeyAsset.DisplayName : "null";
            return $"{enemyName}を{_requiredKillCount}体以上倒す条件";
        }

        [SerializeField, Tooltip("撃破対象となる敵の定義アセット。")] private EnemyMissionKeyAsset _enemyMissionKeyAsset;
        [SerializeField, Tooltip("クリアに必要な撃破数。")] private int _requiredKillCount;
    }
}
