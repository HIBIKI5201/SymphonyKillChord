using DevelopProducts.Design.GameMode.Domain;
using System;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     クリアタイムに関する評価条件。
    ///     例えば、ステージの経過時間が一定時間以内であることなど、クリアタイムに関連する条件を管理するためのクラス。
    /// </summary>
    [Serializable]
    public class ClearTimeEvaluationCondition : IEvaluationCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            return context.StageTimeState.ElapsedTime <= _clearTimeThreshold;
        }

        public string GetDescription()
        {
            return $"クリアタイムが{_clearTimeThreshold}秒以下";
        }

        [SerializeField] private float _clearTimeThreshold;
    }
}