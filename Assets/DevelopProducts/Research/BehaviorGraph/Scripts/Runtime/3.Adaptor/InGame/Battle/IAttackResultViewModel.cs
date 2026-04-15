using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor
{
    /// <summary>
    ///     攻撃結果を表示するためのインターフェース。
    /// </summary>
    public interface IAttackResultViewModel
    {
        /// <summary>
        ///     攻撃結果を表示するためのメソッド。
        /// </summary>
        /// <param name="dto"></param>
        void Push(in AttackResultDTO dto);
    }
}
