using DevelopProducts.Architecture.Domain;

namespace DevelopProducts.Architecture.Application
{
    /// <summary>
    ///     攻撃修飾子のインターフェース。
    /// </summary>
    public interface IAttackModifier
    {
        /// <summary>
        ///     ダメージを修飾する。
        /// </summary>
        /// <param name="damage"> 元のダメージ。 </param>
        /// <returns> 修飾後のダメージ。 </returns>
        public DamageContext Modify(DamageContext damage);
    }
}
