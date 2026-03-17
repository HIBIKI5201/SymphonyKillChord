using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     リスト内の条件の一つでも失敗した場合、失敗とする条件を組み合わせるためのクラス。
    /// </summary>
    [Serializable]
    public class OrFailConditionGroup : IFailCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            if (_children == null || _children.Count == 0)
            {
                return false;
            }


            for (int i = 0; i < _children.Count; i++)
            {
                IFailCondition child = _children[i];
                if (child != null && child.IsSatisfied(context))
                {
                    return true;
                }
            }

            return false;
        }


        [SerializeReference, SubclassSelector, Tooltip("組み合わせる失敗条件のリスト。")]
        private List<IFailCondition> _children = new();

        public string GetDescription()
        {
            return "いずれかの失敗条件を満たす";
        }
    }
}
