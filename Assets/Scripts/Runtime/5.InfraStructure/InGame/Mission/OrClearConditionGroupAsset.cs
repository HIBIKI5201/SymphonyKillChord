using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class OrClearConditionGroupAsset : MissionClearConditionAssetBase
    {
        [SerializeReference, SubclassSelector]
        private List<MissionClearConditionAssetBase> _children = new();

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
