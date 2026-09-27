using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IBackground の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IBackgroundRepository
    {
        /// <summary>
        ///     ID に対応する背景定義を取得する。見つからない場合は false を返す。
        /// </summary>
        bool TryFindById(BackgroundId id, out BackgroundDefinition background);
    }
}
