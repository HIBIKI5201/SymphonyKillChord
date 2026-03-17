using SymphonyFrameWork.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     複数の失敗条件を組み合わせるためのクラス。
    /// </summary>
    public class AndFailConditionGroup : IFailCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (!_children[i].IsSatisfied(context))
                {
                    return false;
                }
            }

            return true;
        }


        [SerializeReference, SubclassSelector, Tooltip("組み合わせるクリア条件のリスト。")]
        private List<IFailCondition> _children = new();

        public string GetDescription()
        {
            return "すべての失敗条件を満たす";
        }
    }
}
