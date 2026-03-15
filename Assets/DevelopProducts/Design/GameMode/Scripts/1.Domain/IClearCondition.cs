namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     クリア条件を表すインターフェース。
    /// </summary>
    public interface IClearCondition
    {
        /// <summary> 条件を満たしているかどうかを評価するメソッド。 </summary>
        bool IsSatisfied(StageRuntimeContext context);
        /// <summary> 条件の説明を取得するメソッド。 </summary>
        string GetDescription();
    }
}
