using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;

namespace DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃処理の1ステップを表すインターフェース。
    /// </summary>
    public interface IAttackStep
    {
        /// <summary>
        ///     攻撃処理の1ステップを実行するメソッド。
        /// </summary>
        /// <param name="context"></param>
        public AttackStepContext Execute(in AttackStepContext context);
    }
}
