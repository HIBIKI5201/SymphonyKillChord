using DevelopProducts.Design.GameMode.Domain;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     ステージの経過時間に関する失敗条件。
    ///     例えば、ステージの経過時間が一定時間を超えた場合にステージが失敗となる条件を管理するためのクラス。
    /// </summary>
    public class ElapsedTimeFailCondition : IFailCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
           return context.StageTimeState.ElapsedTime >= _timeLimit;
        }

        public string GetDescription()
        {
            return $"{_timeLimit}秒以上超えた";
        }

        [SerializeField] private float _timeLimit;
    }
}
