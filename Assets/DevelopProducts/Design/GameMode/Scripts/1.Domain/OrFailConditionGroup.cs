using DevelopProducts.Design.GameMode.Domain;
using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     リスト内の条件の一つでも失敗した場合、失敗とする条件を組み合わせるためのクラス。
    /// </summary>
    public class OrFailConditionGroup : IFailCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].IsSatisfied(context))
                {
                    return true;
                }
            }

            return false;
        }


        [SerializeReference, SubclassSelector, Tooltip("組み合わせるクリア条件のリスト。")]
        private List<IFailCondition> _children = new();

        public string GetDescription()
        {
            throw new NotImplementedException();
        }
    }
}
