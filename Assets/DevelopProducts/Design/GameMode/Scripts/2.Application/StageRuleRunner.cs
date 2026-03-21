using DevelopProducts.Design.GameMode.Domain;
using System;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Application
{
    public enum StageEndReason
    {
        Clear,
        Fail,
        None
    }

    /// <summary>
    ///     ステージのルールを評価し、ステージのクリアや失敗を判定するクラス。
    /// </summary>
    public class StageRuleRunner
    {
        public StageRuleRunner(StageRuntimeContext runtimeContext,
            IClearCondition clearCondition, 
            IFailCondition failCondition)
        {
            _runtimeContext = runtimeContext;
            _clearCondition = clearCondition;
            _failCondition = failCondition;
        }

        public event Action OnStageCleared;
        public event Action OnStageFailed;

        public StageEndReason EndReason { get; private set; } = StageEndReason.None;
        public bool IsStageEnded => EndReason != StageEndReason.None;

        public void EvaluateStageEnd()
        {
            if (IsStageEnded)
            {
                return;
            }
            if (_failCondition.IsSatisfied(_runtimeContext))
            {
                EndReason = StageEndReason.Fail;
                OnStageFailed?.Invoke();
            }
            else if (_clearCondition.IsSatisfied(_runtimeContext))
            {
                EndReason = StageEndReason.Clear;
                OnStageCleared?.Invoke();
            }
        }

        private readonly StageRuntimeContext _runtimeContext;
        private readonly IClearCondition _clearCondition;
        private readonly IFailCondition _failCondition;
    }
}
