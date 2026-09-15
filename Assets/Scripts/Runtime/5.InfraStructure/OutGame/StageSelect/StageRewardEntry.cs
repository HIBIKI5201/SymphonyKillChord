using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Utility.Identity;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ報酬として付与するリソースと数量の入力値です。
    /// </summary>
    [Serializable]
    public struct StageRewardEntry
    {
        /// <summary> 付与するリソースIDの値です。未設定の場合は0です。 </summary>
        public int ResourceIdValue => _resourceId.Id;

        /// <summary> 付与する数量です。 </summary>
        public int Amount => _amount;

        [SerializeField, SourceDataCollection(GameResourceIds.COLLECTION_KEY), Tooltip("付与するリソースのID。")]
        private DataID _resourceId;

        [SerializeField, Min(0), Tooltip("付与する数量。")]
        private int _amount;
    }
}
