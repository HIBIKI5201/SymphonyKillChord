using DevelopProducts.BehaviorGraph.Runtime.Domain;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application
{
    /// <summary>
    ///     スキルの視覚効果をテストするためのクラス。
    /// </summary>
    public class SkillVisualTest : ISkillVisual
    {
        public void Execute()
        {
            Debug.Log("SkillVisual Do");
        }
    }
}