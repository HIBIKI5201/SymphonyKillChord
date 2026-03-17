using DevelopProducts.Design.GameMode.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Application
{
    /// <summary>
    ///     評価条件の達成状況を表すクラス。
    /// </summary>
    public class EvaluationResult
    {
        public EvaluationResult(IReadOnlyList<string> achivedDescriptions)
        {
            AchivedDescriptions = achivedDescriptions;
        }


        public IReadOnlyList<string> AchivedDescriptions { get; }
        public int AchivedCount => AchivedDescriptions.Count;
    }

    /// <summary>
    ///     ゲーム終了時の評価条件の達成状況を判定するクラス。
    /// </summary>
    public class EvaluationRunner
    {
        public EvaluationRunner(StageRuntimeContext runtimeContext, IReadOnlyList<IEvaluationCondition> evaluationConditions)
        {
            _runtimeContext = runtimeContext;
            _evaluationConditions = evaluationConditions;
        }

        public EvaluationResult Run()
        {
            List<string> achived= new();

            foreach (var condition in _evaluationConditions)
            {
                if (condition.IsSatisfied(_runtimeContext))
                {
                    achived.Add(condition.GetDescription());
                }
            }

            return new EvaluationResult(achived);
        }

        private readonly StageRuntimeContext _runtimeContext;
        private readonly IReadOnlyList<IEvaluationCondition> _evaluationConditions;
    }
}
