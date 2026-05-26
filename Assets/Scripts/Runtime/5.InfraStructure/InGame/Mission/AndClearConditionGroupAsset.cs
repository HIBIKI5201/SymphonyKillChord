using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     複数のクリア条件を論理積（AND）でまとめるアセットクラス。
    /// </summary>
    [Serializable]
    public class AndClearConditionGroupAsset : MissionClearConditionAssetBase
    {
        /// <summary>
        ///     クリア条件を生成します。
        /// </summary>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create()
        {
            List<IMissionClearCondition> children = new();

            if (_childConditionAssets != null)
            {
                for (int i = 0; i < _childConditionAssets.Count; i++)
                {
                    if (_childConditionAssets[i] != null)
                    {
                        children.Add(_childConditionAssets[i].Create());
                    }
                }
            }

            return new AndClearConditionGroup(children);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            if (_childConditionAssets == null || _childConditionAssets.Count == 0)
            {
                return "ANDクリア条件（未設定）";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("すべて満たすクリア条件");

            for (int i = 0; i < _childConditionAssets.Count; i++)
            {
                if (_childConditionAssets[i] != null)
                {
                    sb.AppendLine($"- {_childConditionAssets[i].GetType().Name}");
                }
            }

            return sb.ToString();
        }

        [SerializeReference, SubclassSelector, Tooltip("結合する子条件のリスト。")]
        private List<MissionClearConditionAssetBase> _childConditionAssets = new();
    }
}
