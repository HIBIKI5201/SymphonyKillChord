using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     行動反復回数クリア条件のアセットクラス。
    /// </summary>
    [Serializable]
    public class ActionRepeatCountClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <summary>
        ///     クリア条件を生成します。
        /// </summary>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create(EnemyMissionKeyRepository missionKeyRepository)
        {
            if (_requiredCount <= 0)
            {
                throw new InvalidOperationException($"{nameof(_requiredCount)} must be greater than 0.");
            }

            return new ActionRepeatCountClearCondition(_actionKind, _requiredCount);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            return $"{_actionKind}を{_requiredCount}回以上行う条件";
        }

        [SerializeField, Tooltip("計測する行動の種別。")] private MissionActionKind _actionKind;
        [SerializeField, Min(1), Tooltip("クリアに必要な発動回数。")] private int _requiredCount = 1;
    }
}
