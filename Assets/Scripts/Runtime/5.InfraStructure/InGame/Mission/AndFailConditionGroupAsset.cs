using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     複数の失敗条件を論理積（AND）でまとめるアセットクラス。
    /// </summary>
    [Serializable]
    public class AndFailConditionGroupAsset : MissionFailConditionAssetBase
    {
        /// <summary>
        ///     失敗条件を生成します。
        /// </summary>
        /// <returns>失敗条件。</returns>
        public override IMissionFailCondition Create()
        {
            List<IMissionFailCondition> children = new();

            for (int i = 0; i < _childConditionAssets.Count; i++)
            {
                if (_childConditionAssets[i] != null)
                {
                    children.Add(_childConditionAssets[i].Create());
                }
            }

            return new AndFailConditionGroup(children);
        }

        /// <summary> サマリーを構築します。 </summary>
        /// <returns> サマリー文字列。 </returns>
        protected override string BuildSummary()
        {
            if (_childConditionAssets == null || _childConditionAssets.Count == 0)
            {
                return "AND失敗条件（未設定）";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("すべて満たす失敗条件");

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
        private List<MissionFailConditionAssetBase> _childConditionAssets = new();
    }
}
