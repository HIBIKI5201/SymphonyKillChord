namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     現在の所持ポイントを取得するポートインターフェース。
    ///     Infrastructure 層で JSON 読み込み実装を提供する。
    /// </summary>
    public interface IPointRepository
    {
        /// <summary>総獲得ポイント − 使用済みポイント = 現在の所持ポイント</summary>
        UnlockPoint GetCurrentPoints();
        void UsePoints(int points);
    }
}
