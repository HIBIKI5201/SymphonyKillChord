namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     使用回数を消費する状態効果を表すインターフェース。
    /// </summary>
    public interface IConsumableStatusEffect
    {
        /// <summary> 状態効果が消費済みかどうかを取得します。 </summary>
        bool IsConsumed { get; }
    }
}
