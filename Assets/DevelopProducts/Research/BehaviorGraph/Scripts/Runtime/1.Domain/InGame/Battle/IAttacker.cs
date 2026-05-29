using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;

namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃者を表すインターフェース。
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        ///     攻撃力を表すプロパティ。
        /// </summary>
        public AttackPower AttackPower { get; }
    }
}
