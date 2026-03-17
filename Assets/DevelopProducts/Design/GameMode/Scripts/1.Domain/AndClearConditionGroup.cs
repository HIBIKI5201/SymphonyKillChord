using SymphonyFrameWork.Attribute;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     複数のクリア条件を組み合わせるためのクラス。
    ///     例えば、特定の敵を一定数撃破することと、ステージの経過時間が一定時間以内であることなど、複数の条件をAND条件で組み合わせて管理する。
    /// </summary>
    [Serializable]
    public class AndClearConditionGroup : IClearCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            // 未設定時は常にクリアしないようにする。
            if (_children.Count == 0 || _children.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < _children.Count; i++)
            {
                IClearCondition child = _children[i];
                if (child == null || !child.IsSatisfied(context))
                {
                    return false;
                }
            }

            return true;
        }


        [SerializeReference, SubclassSelector, Tooltip("組み合わせるクリア条件のリスト。")]
        private List<IClearCondition> _children = new();

        public string GetDescription()
        {
            return "すべてのクリア条件を満たす";
        }

    }
}
