using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.InfraStructure.InGame.Buff;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     内側のクリア条件をラップし、ステップの間だけプレイヤーへバフ・デバフを付与するクリア条件のアセットです。
    /// </summary>
    [Serializable]
    public sealed class PlayerBuffClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <inheritdoc />
        public override IMissionClearCondition Create()
        {
            if (_innerCondition == null)
            {
                throw new InvalidOperationException($"{nameof(_innerCondition)} is required.");
            }

            List<IBuff> buffs = new();
            if (_buffs != null)
            {
                for (int i = 0; i < _buffs.Count; i++)
                {
                    if (_buffs[i] != null)
                    {
                        buffs.Add(_buffs[i].Create());
                    }
                }
            }

            return new PlayerBuffClearCondition(_innerCondition.Create(), buffs);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            string innerName = _innerCondition != null ? _innerCondition.GetType().Name : "未設定";
            int buffCount = _buffs?.Count ?? 0;
            return $"プレイヤーBuff付与({buffCount}件) + {innerName}";
        }

        [SerializeReference, SubclassSelector, Tooltip("達成判定を委譲する内側のクリア条件。")]
        private MissionClearConditionAssetBase _innerCondition;

        [SerializeReference, SubclassSelector, Tooltip("このステップの間だけプレイヤーへ付与するバフ・デバフのリスト。")]
        private List<PlayerBuffDefinitionAssetBase> _buffs = new();
    }
}
