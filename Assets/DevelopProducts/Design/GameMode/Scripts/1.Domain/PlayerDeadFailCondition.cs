using DevelopProducts.Design.GameMode.Domain;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     プレイヤーの死亡に関する失敗条件。
    ///     例えば、プレイヤーが死亡した場合にステージが失敗となる条件を管理するためのクラス。
    /// </summary>
    public class PlayerDeadFailCondition : IFailCondition
    {
        public bool IsSatisfied(StageRuntimeContext context)
        {
            return context.PlayerRuntimeState.IsDead;
        }

        public string GetDescription()
        {
            return "プレイヤーが死亡する";
        }
    }
}
