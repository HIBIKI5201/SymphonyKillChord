using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class AndClearConditionGroupAsset : MissionClearConditionAssetBase
    {
        public override IMissionClearCondition Create()
        {
            List<IMissionClearCondition> children = new();

            for (int i = 0; i < _childConditionAssets.Count; i++)
            {
                if (children[i] != null)
                {
                    children.Add(_childConditionAssets[i].Create());
                }
            }

            return new AndClearConditionGroup(children);
        }

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

        [SerializeReference, SubclassSelector]
        private List<MissionClearConditionAssetBase> _childConditionAssets = new();
    }
}
