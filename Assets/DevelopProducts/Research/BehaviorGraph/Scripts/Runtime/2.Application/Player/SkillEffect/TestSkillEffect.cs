using DevelopProducts.BehaviorGraph.Runtime.Domain;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Application
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class TestSkillEffect : ISkillEffect
    {
        public void Execute()
        {
            Debug.Log("SkillEffect Do");
        }
    }
}