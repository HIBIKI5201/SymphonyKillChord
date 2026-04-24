using KillChord.Runtime.Domain.InGame.Mission.FailCondition;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public class OrFailConditionGroupAsset : MissionFailConditionAssetBase
    {
        [SerializeReference, SubclassSelector]
        private List<MissionFailConditionAssetBase> _children = new();

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
