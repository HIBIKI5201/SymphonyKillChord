using UnityEngine;
namespace DevelopProducts.BehaviorGraph.Runtime.View.InGame
{
    /// <summary>
    ///     行動AI用インタフェース。
    /// </summary>
    public interface IAIFacade
    {

        public void Hold();

        public void ChaseTargert();

        public void AttackTarget();

    }
}