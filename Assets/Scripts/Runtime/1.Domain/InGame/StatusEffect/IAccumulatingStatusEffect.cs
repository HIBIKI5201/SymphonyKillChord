namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     同じ状態効果が再適用されたときに、状態効果を累積することができることを示すインターフェース。
    /// </summary>
    public interface IAccumulatingStatusEffect
    {
        /// <summary>
        ///     再付与された状態効果を累積する。
        /// </summary>
        /// <param name="statusEffect"> 累積する状態効果 </param>
        void Accumulate(IStatusEffect statusEffect);
    }
}
