using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     複数のクリア条件を論理和（OR）でまとめるアセットクラス。
    /// </summary>
    [Serializable]
    public class OrClearConditionGroupAsset : MissionClearConditionAssetBase
    {
        [SerializeReference, SubclassSelector, Tooltip("結合する子条件のリスト。")]
        private List<MissionClearConditionAssetBase> _children = new();

        /// <summary>
        ///     クリア条件を生成します。
        /// </summary>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create()
        {
            List<IMissionClearCondition> children = new();

            if (_children == null)
            {
                return new OrClearConditionGroup(children);
            }
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i] != null)
                {
                    children.Add(_children[i].Create());
                }
            }

            return new OrClearConditionGroup(children);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            if (_children == null || _children.Count == 0)
            {
                return "ORクリア条件（未設定）";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("いずれかを満たすクリア条件");

            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i] != null)
                {
                    sb.AppendLine($"- {_children[i].GetType().Name}");
                }
            }

            return sb.ToString();
        }
    }
}
