using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     複数の失敗条件を論理和（OR）でまとめるアセットクラス。
    /// </summary>
    [Serializable]
    public class OrFailConditionGroupAsset : MissionFailConditionAssetBase
    {
        [SerializeReference, SubclassSelector, Tooltip("結合する子条件のリスト。")]
        private List<MissionFailConditionAssetBase> _children = new();

        /// <summary>
        ///     失敗条件を生成します。
        /// </summary>
        /// <returns>失敗条件。</returns>
        public override IMissionFailCondition Create()
        {
            List<IMissionFailCondition> children = new();

            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i] != null)
                {
                    children.Add(_children[i].Create());
                }
            }

            return new OrFailConditionGroup(children);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            if (_children == null || _children.Count == 0)
            {
                return "OR失敗条件（未設定）";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("いずれかを満たす失敗条件");

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
