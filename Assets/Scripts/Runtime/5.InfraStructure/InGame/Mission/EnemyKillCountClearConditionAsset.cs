using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Utility.Identity;
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
        /// <param name="missionKeyRepository"> 敵ミッションキーの解決に使うリポジトリです。 </param>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create(EnemyMissionKeyRepository missionKeyRepository)
        {
            if (missionKeyRepository == null
                || !missionKeyRepository.TryGetAsset(new EnemyMissionKey(_enemyMissionKeyId.Id), out EnemyMissionKeyAsset asset))
            {
                throw new InvalidOperationException($"{nameof(_enemyMissionKeyId)} に対応する EnemyMissionKeyAsset が見つかりません。");
            }

            if (_requiredKillCount <= 0)
            {
                throw new InvalidOperationException($"{nameof(_requiredKillCount)} must be greater than 0.");
            }

            return new EnemyKillCountClearCondition(
                asset.Id,
                _requiredKillCount,
                asset.DisplayName);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            return $"敵ミッションキー(Id:{_enemyMissionKeyId.Id})を{_requiredKillCount}体以上倒す条件";
        }

        [SerializeField, SourceDataCollection("EnemyMissionKey"), Tooltip("撃破対象となる敵のID。")]
        private DataID _enemyMissionKeyId;
        [SerializeField, Tooltip("クリアに必要な撃破数。")] private int _requiredKillCount;
    }
}
